using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _svc;
    public DashboardController(IDashboardService svc) => _svc = svc;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats() => Ok(await _svc.GetStatsAsync());
}
