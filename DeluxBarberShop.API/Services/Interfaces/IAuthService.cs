using DeluxBarberShop.API.DTOs.Auth;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
}
