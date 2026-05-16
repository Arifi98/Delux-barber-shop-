using DeluxBarberShop.API.DTOs.Dashboard;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IDashboardService
{
    Task<DashboardStatsDto> GetStatsAsync();
}
