using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Dashboard;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardStatsDto> GetStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var todayCount      = await _db.Appointments.CountAsync(a => a.AppointmentDate.Date == today);
        var pendingCount    = await _db.Appointments.CountAsync(a => a.Status == "Pending");
        var confirmedCount  = await _db.Appointments.CountAsync(a => a.Status == "Confirmed");
        var totalClients    = await _db.Clients.CountAsync();
        var activeBarbers   = await _db.Barbers.CountAsync(b => b.IsActive);
        var revenue = await _db.Appointments
            .Include(a => a.Service)
            .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
            .SumAsync(a => (decimal?)a.Service.Price) ?? 0;

        var recent = await _db.Appointments
            .Include(a => a.Service).Include(a => a.Barber)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new RecentAppointmentDto
            {
                Id = a.Id,
                ClientName = $"{a.FirstName} {a.LastName}",
                ServiceName = a.Service.Name,
                BarberName = a.Barber == null ? null : $"{a.Barber.FirstName} {a.Barber.LastName}",
                AppointmentDate = a.AppointmentDate,
                AppointmentTime = a.AppointmentTime.ToString(),
                Status = a.Status
            }).ToListAsync();

        return new DashboardStatsDto
        {
            TodayAppointments  = todayCount,
            PendingAppointments  = pendingCount,
            ConfirmedAppointments = confirmedCount,
            TotalClients         = totalClients,
            ActiveBarbers        = activeBarbers,
            EstimatedRevenue     = revenue,
            RecentAppointments   = recent
        };
    }
}
