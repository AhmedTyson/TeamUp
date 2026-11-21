// Date: 2026-11-21
// Purpose: Production SMTP email sender delivering password reset links via TLS SMTP with zero in-memory token retention (STRIDE THR-S-03).

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;

namespace TeamUp.Infrastructure.Services;

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> emailOptions,
    IOptions<JwtOptions> jwtOptions,
    ITemplateRenderer templateRenderer,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _email = emailOptions.Value;
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_email.SmtpHost))
            throw new InvalidOperationException("Email:SmtpHost must be configured when Email:Provider is Smtp.");

        var separator = _email.ResetPasswordUrlBase.Contains('?') ? "&" : "?";
        var resetUrl = $"{_email.ResetPasswordUrlBase}{separator}email={Uri.EscapeDataString(toEmail)}&token={Uri.EscapeDataString(resetToken)}";

        var htmlBody = await templateRenderer.RenderEmailAsync(
            "PasswordReset.html",
            new Dictionary<string, string>
            {
                ["Email"] = toEmail,
                ["ResetUrl"] = resetUrl,
                ["ResetToken"] = resetToken,
                ["ExpiresMinutes"] = _jwt.PasswordResetTokenMinutes.ToString()
            },
            ct);

        using var message = new MailMessage
        {
            From = new MailAddress(_email.FromAddress, _email.FromName),
            Subject = "Reset your TeamUp password",
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail));

        using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort)
        {
            EnableSsl = _email.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_email.Username))
        {
            client.Credentials = new NetworkCredential(_email.Username, _email.Password);
        }

        await client.SendMailAsync(message, ct);
        logger.LogInformation("Dispatched password reset email via SMTP to {Email}", toEmail);
    }
}
