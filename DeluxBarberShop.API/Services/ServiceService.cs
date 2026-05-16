using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Service;
using DeluxBarberShop.API.Models;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _db;
    public ServiceService(AppDbContext db) => _db = db;

    public async Task<List<ServiceDto>> GetAllAsync() =>
        await _db.Services.OrderBy(s => s.Name).Select(s => ToDto(s)).ToListAsync();

    public async Task<ServiceDto?> GetByIdAsync(int id)
    {
        var s = await _db.Services.FindAsync(id);
        return s == null ? null : ToDto(s);
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceDto dto)
    {
        var s = new Service { Name = dto.Name, Description = dto.Description, Price = dto.Price, DurationMinutes = dto.DurationMinutes, IsActive = dto.IsActive };
        _db.Services.Add(s);
        await _db.SaveChangesAsync();
        return ToDto(s);
    }

    public async Task<ServiceDto?> UpdateAsync(int id, CreateServiceDto dto)
    {
        var s = await _db.Services.FindAsync(id);
        if (s == null) return null;
        s.Name = dto.Name; s.Description = dto.Description; s.Price = dto.Price;
        s.DurationMinutes = dto.DurationMinutes; s.IsActive = dto.IsActive; s.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(s);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var s = await _db.Services.FindAsync(id);
        if (s == null) return false;
        _db.Services.Remove(s);
        await _db.SaveChangesAsync();
        return true;
    }

    private static ServiceDto ToDto(Service s) => new()
    {
        Id = s.Id, Name = s.Name, Description = s.Description,
        Price = s.Price, DurationMinutes = s.DurationMinutes,
        IsActive = s.IsActive, CreatedAt = s.CreatedAt
    };
}
