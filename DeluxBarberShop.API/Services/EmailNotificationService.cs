using System.Globalization;
using DeluxBarberShop.API.Models;
using DeluxBarberShop.API.Options;
using DeluxBarberShop.API.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DeluxBarberShop.API.Services;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(IOptions<EmailOptions> options, ILogger<EmailNotificationService> logger)
    {
        _options = options.Value;
        _logger  = logger;
    }

    public async Task SendAppointmentConfirmedAsync(Appointment appointment)
    {
        if (!_options.Enabled) return;
        var email = appointment.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return;

        var client  = $"{appointment.FirstName} {appointment.LastName}".Trim();
        var date    = appointment.AppointmentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var time    = appointment.AppointmentTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        var service = appointment.Service?.Name ?? "Shërbim";
        var barber  = appointment.Barber == null ? string.Empty
            : $"<tr style='background:#fff;border-top:1px solid #2a2a2a;'><td style='padding:10px 16px;color:#888;font-size:13px;'>✂️&nbsp;Barberi</td><td style='padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;'>{appointment.Barber.FirstName} {appointment.Barber.LastName}</td></tr>";
        var appUrl  = (_options.AppUrl ?? "").TrimEnd('/');
        var statusLink = !string.IsNullOrWhiteSpace(appointment.BookingToken) && !string.IsNullOrWhiteSpace(appUrl)
            ? $"<p style='margin:28px 0 0;text-align:center;'><a href='{appUrl}/status/{appointment.BookingToken}' style='display:inline-block;background:linear-gradient(135deg,#c9a84c,#f0c040);color:#000;text-decoration:none;padding:14px 36px;border-radius:8px;font-weight:700;font-size:15px;'>Shiko rezervimin →</a></p>"
            : string.Empty;

        var subject  = $"Rezervimi konfirmuar – {date} ora {time}";
        var htmlBody = BuildHtml(client, $"""
            <p style="margin:0 0 20px;font-size:15px;color:#ccc;line-height:1.6;">Rezervimi juaj te <strong style="color:#f5f5f5;">Delux Barber Shop</strong> u konfirmua me sukses!</p>
            <table width="100%" cellpadding="0" cellspacing="0" style="border-radius:8px;overflow:hidden;border:1px solid #2a2a2a;">
              <tr style="background:#1a1a1a;"><td style="padding:10px 16px;color:#888;font-size:13px;width:110px;">📅&nbsp;Data</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{date}</td></tr>
              <tr style="background:#141414;border-top:1px solid #2a2a2a;"><td style="padding:10px 16px;color:#888;font-size:13px;">🕐&nbsp;Ora</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{time}</td></tr>
              <tr style="background:#1a1a1a;border-top:1px solid #2a2a2a;"><td style="padding:10px 16px;color:#888;font-size:13px;">✂️&nbsp;Shërbimi</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{service}</td></tr>
              {barber}
            </table>
            {statusLink}
            <p style="margin:24px 0 0;font-size:13px;color:#666;text-align:center;">Nëse nuk mund të vini, ju lutem anuloni të paktën 2 orë para.</p>
            """);

        await SendAsync(email, subject, htmlBody, appointment.Id, "confirmation");
    }

    public async Task SendAppointmentReminderAsync(Appointment appointment)
    {
        if (!_options.Enabled) return;
        var email = appointment.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return;

        var client  = $"{appointment.FirstName} {appointment.LastName}".Trim();
        var date    = appointment.AppointmentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var time    = appointment.AppointmentTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        var service = appointment.Service?.Name ?? "Shërbim";
        var subject = $"Kujtesë – nesër {date} ora {time}";
        var htmlBody = BuildHtml(client, $"""
            <p style="margin:0 0 20px;font-size:15px;color:#ccc;line-height:1.6;">Ju kujtojmë se keni rezervim <strong style="color:#f5f5f5;">nesër</strong> te Delux Barber Shop.</p>
            <table width="100%" cellpadding="0" cellspacing="0" style="border-radius:8px;overflow:hidden;border:1px solid #2a2a2a;">
              <tr style="background:#1a1a1a;"><td style="padding:10px 16px;color:#888;font-size:13px;width:110px;">📅&nbsp;Data</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{date}</td></tr>
              <tr style="background:#141414;border-top:1px solid #2a2a2a;"><td style="padding:10px 16px;color:#888;font-size:13px;">🕐&nbsp;Ora</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{time}</td></tr>
              <tr style="background:#1a1a1a;border-top:1px solid #2a2a2a;"><td style="padding:10px 16px;color:#888;font-size:13px;">✂️&nbsp;Shërbimi</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{service}</td></tr>
            </table>
            <p style="margin:24px 0 0;font-size:13px;color:#666;text-align:center;">Nëse nuk mund të vini, ju lutem njoftoni klinikën.</p>
            """);

        await SendAsync(email, subject, htmlBody, appointment.Id, "reminder");
    }

    public async Task SendAppointmentRescheduledAsync(Appointment appointment)
    {
        if (!_options.Enabled) return;
        var email = appointment.Email?.Trim();
        if (string.IsNullOrWhiteSpace(email)) return;

        var client  = $"{appointment.FirstName} {appointment.LastName}".Trim();
        var date    = appointment.AppointmentDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var time    = appointment.AppointmentTime.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        var subject = $"Rezervimi ridatua – {date} ora {time}";
        var htmlBody = BuildHtml(client, $"""
            <p style="margin:0 0 20px;font-size:15px;color:#ccc;">Rezervimi juaj te <strong style="color:#f5f5f5;">Delux Barber Shop</strong> u ridatua.</p>
            <table width="100%" cellpadding="0" cellspacing="0" style="border-radius:8px;overflow:hidden;border:1px solid #2a2a2a;">
              <tr style="background:#1a1a1a;"><td style="padding:10px 16px;color:#888;font-size:13px;width:110px;">📅&nbsp;Data e re</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{date}</td></tr>
              <tr style="background:#141414;border-top:1px solid #2a2a2a;"><td style="padding:10px 16px;color:#888;font-size:13px;">🕐&nbsp;Ora e re</td><td style="padding:10px 16px;font-size:14px;font-weight:700;color:#f5f5f5;">{time}</td></tr>
            </table>
            """);

        await SendAsync(email, subject, htmlBody, appointment.Id, "reschedule");
    }

    private async Task SendAsync(string to, string subject, string htmlBody, int appointmentId, string type)
    {
        if (string.IsNullOrWhiteSpace(_options.SmtpHost)) return;
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.MessageId = $"{Guid.NewGuid():N}@deluxbarbershop.com";
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var smtp = new SmtpClient();
            var opts = _options.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, opts, cts.Token);
            await smtp.AuthenticateAsync(_options.Username, _options.Password, cts.Token);
            await smtp.SendAsync(message, cts.Token);
            await smtp.DisconnectAsync(true, cts.Token);
            _logger.LogInformation("Email {Type} sent for appointment {Id}.", type, appointmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email {Type} failed for appointment {Id}.", type, appointmentId);
        }
    }

    private static string BuildHtml(string clientName, string content) => $"""
        <!DOCTYPE html><html lang="sq"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1.0"></head>
        <body style="margin:0;padding:0;background:#0a0a0a;font-family:'Segoe UI',Helvetica,Arial,sans-serif;">
        <table width="100%" cellpadding="0" cellspacing="0" style="background:#0a0a0a;padding:32px 16px;">
          <tr><td align="center">
            <table width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;">
              <tr><td style="background:linear-gradient(135deg,#1a1a1a,#000);border-radius:12px 12px 0 0;padding:20px 28px;">
                <table width="100%" cellpadding="0" cellspacing="0"><tr>
                  <td style="font-size:22px;vertical-align:middle;padding-right:12px;">✂️</td>
                  <td><p style="margin:0;font-size:18px;font-weight:800;color:#f5f5f5;">Delux Barber Shop</p>
                      <p style="margin:2px 0 0;font-size:11px;color:#c9a84c;letter-spacing:1px;text-transform:uppercase;">Premium Barber Experience</p></td>
                </tr></table>
              </td></tr>
              <tr><td style="background:linear-gradient(90deg,#c9a84c,#f0c040,#c9a84c);height:3px;font-size:0;">&nbsp;</td></tr>
              <tr><td style="background:#111;padding:32px 36px;">
                <h2 style="margin:0 0 20px;font-size:20px;color:#f5f5f5;font-weight:800;">Përshëndetje, {clientName}!</h2>
                {content}
              </td></tr>
              <tr><td style="background:#0d0d0d;border-top:1px solid #1a1a1a;border-radius:0 0 12px 12px;padding:16px 36px;text-align:center;">
                <p style="margin:0;font-size:12px;color:#555;">Delux Barber Shop &bull; info@deluxbarbershop.com</p>
              </td></tr>
            </table>
          </td></tr>
        </table>
        </body></html>
        """;
}
