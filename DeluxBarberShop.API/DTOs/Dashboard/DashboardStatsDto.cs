namespace DeluxBarberShop.API.DTOs.Dashboard;
public class DashboardStatsDto
{
    public int TodayAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int TotalClients { get; set; }
    public int ActiveBarbers { get; set; }
    public decimal EstimatedRevenue { get; set; }
    public List<RecentAppointmentDto> RecentAppointments { get; set; } = new();
}

public class RecentAppointmentDto
{
    public int Id { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string? BarberName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
