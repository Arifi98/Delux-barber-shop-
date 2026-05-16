using System.ComponentModel.DataAnnotations;
namespace DeluxBarberShop.API.DTOs.Appointment;
public class UpdateAppointmentDto
{
    public int? ClientId { get; set; }
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    [Required] public int ServiceId { get; set; }
    public int? BarberId { get; set; }
    [Required] public DateTime AppointmentDate { get; set; }
    [Required] public string AppointmentTime { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
}
