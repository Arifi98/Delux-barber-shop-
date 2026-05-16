using System.ComponentModel.DataAnnotations;
namespace DeluxBarberShop.API.DTOs.Barber;
public class CreateBarberDto
{
    [Required] public string FirstName { get; set; } = string.Empty;
    [Required] public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Bio { get; set; }
    public string? Specialty { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
