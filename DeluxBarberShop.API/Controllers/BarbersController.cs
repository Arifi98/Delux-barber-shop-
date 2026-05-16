using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeluxBarberShop.API.DTOs.Barber;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Controllers;

[ApiController]
[Route("api/barbers")]
public class BarbersController : ControllerBase
{
    private readonly IBarberService _svc;
    public BarbersController(IBarberService svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _svc.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _svc.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpPost, Authorize]
    public async Task<IActionResult> Create([FromBody] CreateBarberDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        return Ok(await _svc.CreateAsync(dto));
    }

    [HttpPut("{id}"), Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBarberDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _svc.UpdateAsync(id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}"), Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _svc.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
