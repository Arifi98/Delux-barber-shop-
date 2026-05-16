using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _svc;
    public ClientsController(IClientService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search) => Ok(await _svc.GetAllAsync(search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }
}
