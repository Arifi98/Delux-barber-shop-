using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Barber;
using DeluxBarberShop.API.Models;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class BarberService : IBarberService
{
    private readonly AppDbContext _db;
    public BarberService(AppDbContext db) => _db = db;

    public async Task<List<BarberDto>> GetAllAsync() =>
        await _db.Barbers.OrderBy(b => b.LastName).Select(b => ToDto(b)).ToListAsync();

    public async Task<BarberDto?> GetByIdAsync(int id)
    {
        var b = await _db.Barbers.FindAsync(id);
        return b == null ? null : ToDto(b);
    }

    public async Task<BarberDto> CreateAsync(CreateBarberDto dto)
    {
        var b = new Barber { FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email, Phone = dto.Phone, Bio = dto.Bio, Specialty = dto.Specialty, ImageUrl = dto.ImageUrl, IsActive = dto.IsActive };
        _db.Barbers.Add(b);
        await _db.SaveChangesAsync();
        return ToDto(b);
    }

    public async Task<BarberDto?> UpdateAsync(int id, CreateBarberDto dto)
    {
        var b = await _db.Barbers.FindAsync(id);
        if (b == null) return null;
        b.FirstName = dto.FirstName; b.LastName = dto.LastName; b.Email = dto.Email;
        b.Phone = dto.Phone; b.Bio = dto.Bio; b.Specialty = dto.Specialty;
        b.ImageUrl = dto.ImageUrl; b.IsActive = dto.IsActive; b.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ToDto(b);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var b = await _db.Barbers.FindAsync(id);
        if (b == null) return false;
        _db.Barbers.Remove(b);
        await _db.SaveChangesAsync();
        return true;
    }

    private static BarberDto ToDto(Barber b) => new()
    {
        Id = b.Id, FirstName = b.FirstName, LastName = b.LastName,
        Email = b.Email, Phone = b.Phone, Bio = b.Bio,
        Specialty = b.Specialty, ImageUrl = b.ImageUrl,
        IsActive = b.IsActive, CreatedAt = b.CreatedAt
    };
}
