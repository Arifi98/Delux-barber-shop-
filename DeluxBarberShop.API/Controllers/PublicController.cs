using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeluxBarberShop.API.Data;
using DeluxBarberShop.API.DTOs.Public;
using DeluxBarberShop.API.Models;
using DeluxBarberShop.API.Services.Interfaces;

namespace DeluxBarberShop.API.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailNotificationService _email;
    private readonly ILogger<PublicController> _logger;

    private static readonly TimeSpan DayStart = TimeSpan.FromHours(9);
    private static readonly TimeSpan DayEnd   = TimeSpan.FromHours(20);

    public PublicController(AppDbContext db, IEmailNotificationService email, ILogger<PublicController> logger)
    {
        _db     = db;
        _email  = email;
        _logger = logger;
    }

    [HttpGet("services")]
    public async Task<IActionResult> GetServices() =>
        Ok(await _db.Services.Where(s => s.IsActive).OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Description, s.Price, s.DurationMinutes }).ToListAsync());

    [HttpGet("barbers")]
    public async Task<IActionResult> GetBarbers() =>
        Ok(await _db.Barbers.Where(b => b.IsActive).OrderBy(b => b.LastName)
            .Select(b => new { b.Id, FullName = $"{b.FirstName} {b.LastName}", b.Specialty, b.Bio, b.ImageUrl }).ToListAsync());

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability([FromQuery] string date, [FromQuery] int? barberId, [FromQuery] int? serviceId)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
            return BadRequest("Invalid date format. Use yyyy-MM-dd.");

        if (day.DayOfWeek == DayOfWeek.Sunday)
            return Ok(new { closed = true, slots = Array.Empty<string>() });

        var service = serviceId.HasValue ? await _db.Services.FindAsync(serviceId.Value) : null;
        var slotSize = TimeSpan.FromMinutes(service?.DurationMinutes ?? 30);

        var allSlots = new List<string>();
        for (var t = DayStart; t + slotSize <= DayEnd; t += slotSize)
            allSlots.Add(t.ToString(@"hh\:mm"));

        var taken = await _db.Appointments
            .Where(a => a.AppointmentDate.Date == day.Date
                     && a.Status != "Cancelled" && a.Status != "Completed"
                     && (!barberId.HasValue || a.BarberId == barberId))
            .Select(a => a.AppointmentTime).ToListAsync();

        var takenSet = taken.Select(t => t.ToString(@"hh\:mm")).ToHashSet();
        var slots = allSlots.Select(s => new { time = s, available = !takenSet.Contains(s) }).ToList();
        return Ok(new { closed = false, slots });
    }

    [HttpPost("book")]
    public async Task<IActionResult> Book([FromBody] PublicBookingDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var service = await _db.Services.FindAsync(dto.ServiceId);
        if (service is null || !service.IsActive) return BadRequest("Shërbimi nuk ekziston.");

        Barber? barber = null;
        if (dto.BarberId.HasValue)
        {
            barber = await _db.Barbers.FindAsync(dto.BarberId.Value);
            if (barber is null || !barber.IsActive) return BadRequest("Barberi nuk ekziston.");
        }

        if (!TimeSpan.TryParseExact(dto.AppointmentTime, @"hh\:mm", CultureInfo.InvariantCulture, out var apptTime))
            return BadRequest("Formati i orës është i pavlefshëm.");

        var conflict = await _db.Appointments.AnyAsync(a =>
            a.AppointmentDate.Date == dto.AppointmentDate.Date &&
            a.AppointmentTime == apptTime && a.Status != "Cancelled" && a.Status != "Completed" &&
            (!dto.BarberId.HasValue || a.BarberId == dto.BarberId));

        if (conflict) return Conflict("Ora e zgjedhur është e zënë. Zgjidhni një tjetër.");

        var phone = dto.Phone.Trim();
        var email = dto.Email?.Trim();
        var existingClient = await _db.Clients.FirstOrDefaultAsync(c => c.Phone == phone)
            ?? (email != null ? await _db.Clients.FirstOrDefaultAsync(c => c.Email == email) : null);

        if (existingClient is null)
        {
            existingClient = new Client { FirstName = dto.FirstName.Trim(), LastName = dto.LastName.Trim(), Phone = phone, Email = email };
            _db.Clients.Add(existingClient);
            await _db.SaveChangesAsync();
        }

        var token = Guid.NewGuid().ToString("N")[..12].ToUpper();
        var appointment = new Appointment
        {
            ClientId        = existingClient.Id,
            FirstName       = dto.FirstName.Trim(), LastName = dto.LastName.Trim(),
            Phone           = phone, Email = email,
            ServiceId       = dto.ServiceId, BarberId = dto.BarberId,
            AppointmentDate = dto.AppointmentDate.Date,
            AppointmentTime = apptTime, Status = "Pending",
            Notes           = dto.Notes?.Trim(), BookingToken = token, IsPublicBooking = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();

        var full = await _db.Appointments.Include(a => a.Service).Include(a => a.Barber).FirstAsync(a => a.Id == appointment.Id);
        _ = Task.Run(async () => { try { await _email.SendAppointmentConfirmedAsync(full); } catch { } });

        return Ok(new BookingResultDto
        {
            BookingToken = token, FirstName = dto.FirstName.Trim(),
            ServiceName = service.Name, AppointmentDate = dto.AppointmentDate.Date,
            AppointmentTime = dto.AppointmentTime,
            BarberName = barber == null ? null : $"{barber.FirstName} {barber.LastName}"
        });
    }

    [HttpGet("status/{token}")]
    public async Task<IActionResult> GetStatus(string token)
    {
        var a = await _db.Appointments.Include(x => x.Service).Include(x => x.Barber)
            .FirstOrDefaultAsync(x => x.BookingToken == token.ToUpper());
        if (a is null) return NotFound("Rezervimi nuk u gjet.");
        return Ok(new {
            token = token.ToUpper(), a.FirstName, a.LastName, a.Status,
            a.AppointmentDate, AppointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
            ServiceName = a.Service?.Name, BarberName = a.Barber == null ? null : $"{a.Barber.FirstName} {a.Barber.LastName}",
            a.Notes, a.CreatedAt,
            CanCancel = a.Status is "Pending" or "Confirmed" && (a.AppointmentDate.Date + a.AppointmentTime) > DateTime.UtcNow.AddHours(2)
        });
    }

    [HttpPost("cancel/{token}")]
    public async Task<IActionResult> Cancel(string token)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.BookingToken == token.ToUpper());
        if (a is null) return NotFound("Rezervimi nuk u gjet.");
        if (a.Status is "Cancelled") return BadRequest("Rezervimi është tashmë i anuluar.");
        if ((a.AppointmentDate.Date + a.AppointmentTime) <= DateTime.UtcNow.AddHours(2))
            return BadRequest("Nuk mund të anuloni brenda 2 orëve.");
        a.Status = "Cancelled"; a.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Rezervimi u anulua." });
    }
}
