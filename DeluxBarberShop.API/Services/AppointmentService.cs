using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Appointment;
using DeluxBarberShop.API.Hubs;
using DeluxBarberShop.API.Models;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    private readonly IEmailNotificationService _email;
    private readonly IHubContext<BarberShopHub> _hub;

    public AppointmentService(AppDbContext db, IEmailNotificationService email, IHubContext<BarberShopHub> hub)
    {
        _db    = db;
        _email = email;
        _hub   = hub;
    }

    public async Task<List<AppointmentDto>> GetAllAsync(string? status, DateTime? fromDate, DateTime? toDate, int? barberId, string? search)
    {
        var q = _db.Appointments.Include(a => a.Service).Include(a => a.Barber).AsQueryable();
        if (!string.IsNullOrEmpty(status))    q = q.Where(a => a.Status == status);
        if (fromDate.HasValue)                q = q.Where(a => a.AppointmentDate.Date >= fromDate.Value.Date);
        if (toDate.HasValue)                  q = q.Where(a => a.AppointmentDate.Date <= toDate.Value.Date);
        if (barberId.HasValue)                q = q.Where(a => a.BarberId == barberId);
        if (!string.IsNullOrEmpty(search))    q = q.Where(a => a.FirstName.Contains(search) || a.LastName.Contains(search) || a.Phone.Contains(search));
        return await q.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.AppointmentTime).Select(a => ToDto(a)).ToListAsync();
    }

    public async Task<AppointmentDto?> GetByIdAsync(int id)
    {
        var a = await _db.Appointments.Include(a => a.Service).Include(a => a.Barber).FirstOrDefaultAsync(a => a.Id == id);
        return a == null ? null : ToDto(a);
    }

    public async Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto)
    {
        var time = TimeSpan.TryParse(dto.AppointmentTime, out var t) ? t : TimeSpan.Zero;
        var a = new Appointment
        {
            ClientId        = dto.ClientId,
            FirstName       = dto.FirstName,
            LastName        = dto.LastName,
            Phone           = dto.Phone,
            Email           = dto.Email,
            ServiceId       = dto.ServiceId,
            BarberId        = dto.BarberId,
            AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate.Date, DateTimeKind.Utc),
            AppointmentTime = time,
            Notes           = dto.Notes,
            Status          = dto.Status,
            BookingToken    = Guid.NewGuid().ToString("N")[..12].ToUpper()
        };
        _db.Appointments.Add(a);
        await _db.SaveChangesAsync();
        await _db.Entry(a).Reference(x => x.Service).LoadAsync();
        if (a.BarberId.HasValue) await _db.Entry(a).Reference(x => x.Barber).LoadAsync();
        await _hub.Clients.All.SendAsync("appointmentChanged", new { action = "created", appointmentId = a.Id });
        return ToDto(a);
    }

    public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var a = await _db.Appointments.Include(a => a.Service).Include(a => a.Barber).FirstOrDefaultAsync(a => a.Id == id);
        if (a == null) return null;
        var prevStatus = a.Status;
        var prevDate   = a.AppointmentDate.Date;
        var prevTime   = a.AppointmentTime;
        var time = TimeSpan.TryParse(dto.AppointmentTime, out var t) ? t : a.AppointmentTime;
        a.ClientId = dto.ClientId; a.FirstName = dto.FirstName; a.LastName = dto.LastName;
        a.Phone = dto.Phone; a.Email = dto.Email; a.ServiceId = dto.ServiceId;
        a.BarberId = dto.BarberId; a.AppointmentDate = DateTime.SpecifyKind(dto.AppointmentDate.Date, DateTimeKind.Utc);
        a.AppointmentTime = time; a.Status = dto.Status; a.Notes = dto.Notes;
        a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        await _db.Entry(a).Reference(x => x.Service).LoadAsync();
        if (a.BarberId.HasValue) await _db.Entry(a).Reference(x => x.Barber).LoadAsync();
        var dateChanged = a.AppointmentDate.Date != prevDate || a.AppointmentTime != prevTime;
        if (ShouldSendConfirmation(prevStatus, a.Status))
            await _email.SendAppointmentConfirmedAsync(a);
        else if (dateChanged && a.Status is not ("Cancelled" or "Completed"))
            await _email.SendAppointmentRescheduledAsync(a);
        await _hub.Clients.All.SendAsync("appointmentChanged", new { action = "updated", appointmentId = a.Id });
        return ToDto(a);
    }

    public async Task<AppointmentDto?> UpdateStatusAsync(int id, string status)
    {
        var a = await _db.Appointments.Include(a => a.Service).Include(a => a.Barber).FirstOrDefaultAsync(a => a.Id == id);
        if (a == null) return null;
        var prevStatus = a.Status;
        a.Status = status; a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (ShouldSendConfirmation(prevStatus, a.Status))
            await _email.SendAppointmentConfirmedAsync(a);
        await _hub.Clients.All.SendAsync("appointmentChanged", new { action = "statusChanged", appointmentId = a.Id });
        return ToDto(a);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var a = await _db.Appointments.FindAsync(id);
        if (a == null) return false;
        _db.Appointments.Remove(a);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("appointmentChanged", new { action = "deleted", appointmentId = id });
        return true;
    }

    private static bool ShouldSendConfirmation(string prev, string next) =>
        prev != "Confirmed" && next == "Confirmed";

    private static AppointmentDto ToDto(Appointment a) => new()
    {
        Id = a.Id, ClientId = a.ClientId,
        FirstName = a.FirstName, LastName = a.LastName,
        Phone = a.Phone, Email = a.Email,
        ServiceId = a.ServiceId, ServiceName = a.Service?.Name ?? "",
        ServicePrice = a.Service?.Price ?? 0,
        BarberId = a.BarberId, BarberName = a.Barber == null ? null : $"{a.Barber.FirstName} {a.Barber.LastName}",
        AppointmentDate = a.AppointmentDate,
        AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
        Status = a.Status, Notes = a.Notes,
        ReminderSent = a.ReminderSent, BookingToken = a.BookingToken,
        IsPublicBooking = a.IsPublicBooking,
        CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };
}
