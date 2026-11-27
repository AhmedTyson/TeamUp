// Date: 2026-11-21
// Purpose: Production SMTP email sender delivering password reset links via TLS SMTP with zero in-memory token retention (STRIDE THR-S-03).

using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamUp.Application.Common.Interfaces;
using TeamUp.Application.Common.Security;
using TeamUp.Infrastructure.Templates;

namespace TeamUp.Infrastructure.Services;

public sealed partial class SmtpEmailSender(
    IOptions<EmailOptions> emailOptions,
    IOptions<JwtOptions> jwtOptions,
    ITemplateRenderer templateRenderer,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    #region Logging

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatched password reset email via SMTP to {Email}")]
    private static partial void LogPasswordResetSent(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatched email verification message via SMTP to {Email}")]
    private static partial void LogEmailVerificationSent(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatched templated email '{Subject}' with {AttachmentCount} attachment(s) via SMTP to {Email}")]
    private static partial void LogTemplatedEmailSent(ILogger logger, string subject, int attachmentCount, string email);

    private readonly EmailOptions _email = emailOptions.Value;
    private readonly JwtOptions _jwt = jwtOptions.Value;

    #endregion

    #region Email Dispatch

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

        var plainText = $"Reset your TeamUp password\nAccount: {toEmail}\nLink (valid for {_jwt.PasswordResetTokenMinutes} minutes): {resetUrl}\nFallback Token: {resetToken}";

        using var message = BuildMailMessage(
            _email.FromAddress,
            _email.FromName,
            toEmail,
            "Reset your TeamUp password",
            htmlBody,
            plainText,
            attachments: null,
            _email.FrontendBaseUrl);

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
        LogPasswordResetSent(logger, toEmail);
    }

    public async Task SendEmailVerificationAsync(EmailVerificationMessage message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_email.SmtpHost))
            throw new InvalidOperationException("Email:SmtpHost must be configured when Email:Provider is Smtp.");

        var verifyBase = _email.ResolveVerifyEmailUrlBase();
        var separator = verifyBase.Contains('?') ? "&" : "?";
        var verifyUrl = $"{verifyBase}{separator}email={Uri.EscapeDataString(message.ToEmail)}&token={Uri.EscapeDataString(message.VerificationToken)}";

        var htmlBody = await templateRenderer.RenderEmailAsync(
            "EmailVerification.html",
            new Dictionary<string, string>
            {
                ["FirstName"] = message.FirstName,
                ["UniversityName"] = message.UniversityName,
                ["FacultyName"] = message.FacultyName,
                ["Email"] = message.ToEmail,
                ["VerifyUrl"] = verifyUrl,
                ["ExpiresMinutes"] = message.ExpiresMinutes.ToString(),
                ["VerificationCode"] = message.VerificationToken
            },
            ct);

        var plainText = $"Verify your TeamUp campus email\nHello {message.FirstName},\nAccount: {message.ToEmail}\nInstitution: {message.UniversityName} ({message.FacultyName})\nLink (valid for {message.ExpiresMinutes} minutes): {verifyUrl}\nVerification Code: {message.VerificationToken}";

        using var mail = BuildMailMessage(
            _email.FromAddress,
            _email.FromName,
            message.ToEmail,
            "Verify your TeamUp campus email",
            htmlBody,
            plainText,
            attachments: null,
            _email.FrontendBaseUrl);

        using var client = new SmtpClient(_email.SmtpHost, _email.SmtpPort)
        {
            EnableSsl = _email.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_email.Username))
        {
            client.Credentials = new NetworkCredential(_email.Username, _email.Password);
        }

        await client.SendMailAsync(mail, ct);
        LogEmailVerificationSent(logger, message.ToEmail);
    }

    public async Task SendTemplatedEmailAsync(EmailDispatchRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_email.SmtpHost))
            throw new InvalidOperationException("Email:SmtpHost must be configured when Email:Provider is Smtp.");

        using var message = BuildMailMessage(
            _email.FromAddress,
            _email.FromName,
            request.ToEmail,
            request.Subject,
            request.HtmlBody,
            request.PlainTextBody,
            request.Attachments,
            _email.FrontendBaseUrl);

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
        LogTemplatedEmailSent(logger, request.Subject, request.Attachments?.Count ?? 0, request.ToEmail);
    }

    #endregion

    #region Message Construction

    internal static MailMessage BuildMailMessage(
        string fromAddress,
        string fromName,
        string toEmail,
        string subject,
        string htmlBody,
        string? plainTextBody,
        IReadOnlyList<EmailAttachment>? attachments,
        string? frontendBaseUrl)
    {
        var message = new MailMessage
        {
            From = new MailAddress(fromAddress, fromName),
            Subject = subject
        };
        try
        {
            message.To.Add(new MailAddress(toEmail));

            if (!string.IsNullOrWhiteSpace(plainTextBody))
            {
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                    plainTextBody, Encoding.UTF8, MediaTypeNames.Text.Plain));
            }

            var cidUrl = $"cid:{BrandAssets.EmailLogoContentId}";
            var publicLogoUrl = BrandAssets.BuildPublicEmailLogoUrl(frontendBaseUrl);
            var processedHtml = htmlBody
                .Replace(publicLogoUrl, cidUrl, StringComparison.OrdinalIgnoreCase)
                .Replace(BrandAssets.EmailLogoDataUri, cidUrl, StringComparison.Ordinal);

            var htmlView = AlternateView.CreateAlternateViewFromString(
                processedHtml, Encoding.UTF8, MediaTypeNames.Text.Html);

            if (processedHtml.Contains(cidUrl, StringComparison.OrdinalIgnoreCase))
            {
                var logoStream = new MemoryStream(BrandAssets.EmailLogoPngBytes, writable: false);
                var inlineLogo = new LinkedResource(logoStream, MediaTypeNames.Image.Png)
                {
                    ContentId = BrandAssets.EmailLogoContentId,
                    TransferEncoding = TransferEncoding.Base64
                };
                inlineLogo.ContentType.Name = "teamup-logo.png";
                htmlView.LinkedResources.Add(inlineLogo);
            }

            message.AlternateViews.Add(htmlView);

            if (attachments is { Count: > 0 })
            {
                foreach (var attachment in attachments)
                {
                    var stream = new MemoryStream(attachment.Content, writable: false);
                    message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                }
            }

            return message;
        }
        catch
        {
            message.Dispose();
            throw;
        }
    }

    #endregion
}
