using DeluxBarberShop.API.DTOs.Service;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IServiceService
{
    Task<List<ServiceDto>> GetAllAsync();
    Task<ServiceDto?> GetByIdAsync(int id);
    Task<ServiceDto> CreateAsync(CreateServiceDto dto);
    Task<ServiceDto?> UpdateAsync(int id, CreateServiceDto dto);
    Task<bool> DeleteAsync(int id);
}
