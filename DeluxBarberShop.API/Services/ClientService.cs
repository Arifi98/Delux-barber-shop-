using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Client;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _db;
    public ClientService(AppDbContext db) => _db = db;

    public async Task<List<ClientDto>> GetAllAsync(string? search)
    {
        var q = _db.Clients.Include(c => c.Appointments).AsQueryable();
        if (!string.IsNullOrEmpty(search))
            q = q.Where(c => c.FirstName.Contains(search) || c.LastName.Contains(search) || c.Phone.Contains(search));
        return await q.OrderBy(c => c.LastName).Select(c => new ClientDto
        {
            Id = c.Id, FirstName = c.FirstName, LastName = c.LastName,
            Phone = c.Phone, Email = c.Email, Notes = c.Notes,
            TotalAppointments = c.Appointments.Count, CreatedAt = c.CreatedAt
        }).ToListAsync();
    }

    public async Task<ClientDto?> GetByIdAsync(int id)
    {
        var c = await _db.Clients.Include(c => c.Appointments).FirstOrDefaultAsync(c => c.Id == id);
        if (c == null) return null;
        return new ClientDto { Id = c.Id, FirstName = c.FirstName, LastName = c.LastName, Phone = c.Phone, Email = c.Email, Notes = c.Notes, TotalAppointments = c.Appointments.Count, CreatedAt = c.CreatedAt };
    }
}
