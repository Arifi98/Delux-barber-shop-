namespace DeluxBarberShop.API.DTOs.Client;
public class ClientDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public int TotalAppointments { get; set; }
    public DateTime CreatedAt { get; set; }
}
