using DeluxBarberShop.API.DTOs.Barber;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IBarberService
{
    Task<List<BarberDto>> GetAllAsync();
    Task<BarberDto?> GetByIdAsync(int id);
    Task<BarberDto> CreateAsync(CreateBarberDto dto);
    Task<BarberDto?> UpdateAsync(int id, CreateBarberDto dto);
    Task<bool> DeleteAsync(int id);
}
