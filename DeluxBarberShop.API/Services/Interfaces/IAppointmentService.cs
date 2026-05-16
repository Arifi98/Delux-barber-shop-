using DeluxBarberShop.API.DTOs.Appointment;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IAppointmentService
{
    Task<List<AppointmentDto>> GetAllAsync(string? status, DateTime? fromDate, DateTime? toDate, int? barberId, string? search);
    Task<AppointmentDto?> GetByIdAsync(int id);
    Task<AppointmentDto> CreateAsync(CreateAppointmentDto dto);
    Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
    Task<AppointmentDto?> UpdateStatusAsync(int id, string status);
    Task<bool> DeleteAsync(int id);
}
