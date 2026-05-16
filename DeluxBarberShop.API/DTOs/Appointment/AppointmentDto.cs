namespace DeluxBarberShop.API.DTOs.Appointment;
public class AppointmentDto
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal ServicePrice { get; set; }
    public int? BarberId { get; set; }
    public string? BarberName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; }
    public string? BookingToken { get; set; }
    public bool IsPublicBooking { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
