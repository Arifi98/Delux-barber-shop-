using System.ComponentModel.DataAnnotations;
namespace DeluxBarberShop.API.DTOs.Service;
public class CreateServiceDto
{
    [Required] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0, 9999)] public decimal Price { get; set; }
    [Range(5, 480)] public int DurationMinutes { get; set; } = 30;
    public bool IsActive { get; set; } = true;
}
