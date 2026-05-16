namespace DeluxBarberShop.API.DTOs.Public;
public class BookingResultDto
{
    public string BookingToken { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public string? BarberName { get; set; }
}
