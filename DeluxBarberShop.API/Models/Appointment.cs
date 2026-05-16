namespace DeluxBarberShop.API.Models;
public class Appointment
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public Client? Client { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; } = null!;
    public int? BarberId { get; set; }
    public Barber? Barber { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public bool ReminderSent { get; set; } = false;
    public string? BookingToken { get; set; }
    public bool IsPublicBooking { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
