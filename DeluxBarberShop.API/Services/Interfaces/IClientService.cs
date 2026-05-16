using DeluxBarberShop.API.DTOs.Client;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync(string? search);
    Task<ClientDto?> GetByIdAsync(int id);
}
