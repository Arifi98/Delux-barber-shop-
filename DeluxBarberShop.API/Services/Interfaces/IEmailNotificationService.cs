using DeluxBarberShop.API.Models;
namespace DeluxBarberShop.API.Services.Interfaces;
public interface IEmailNotificationService
{
    Task SendAppointmentConfirmedAsync(Appointment appointment);
    Task SendAppointmentReminderAsync(Appointment appointment);
    Task SendAppointmentRescheduledAsync(Appointment appointment);
}
