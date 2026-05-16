using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(IServiceScopeFactory scopeFactory, ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SendPendingRemindersAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Error in reminder loop."); }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task SendPendingRemindersAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db    = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var email = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var appointments = await db.Appointments
            .Include(a => a.Service).Include(a => a.Barber)
            .Where(a => a.AppointmentDate.Date == tomorrow
                     && !a.ReminderSent
                     && (a.Status == "Pending" || a.Status == "Confirmed")
                     && a.Email != null)
            .ToListAsync(ct);

        foreach (var a in appointments)
        {
            try
            {
                await email.SendAppointmentReminderAsync(a);
                a.ReminderSent = true;
                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex) { _logger.LogError(ex, "Reminder failed for appointment {Id}.", a.Id); }
        }
    }
}
