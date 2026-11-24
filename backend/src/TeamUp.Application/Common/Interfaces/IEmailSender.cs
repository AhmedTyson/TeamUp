// Date: 2026-11-24
// Purpose: Email dispatch contract supporting password reset tokens, templated HTML/plain-text mail, and PDF attachments.

namespace TeamUp.Application.Common.Interfaces;

public sealed record EmailAttachment(
    string FileName,
    string ContentType,
    byte[] Content);

public sealed record EmailDispatchRequest(
    string ToEmail,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null,
    IReadOnlyList<EmailAttachment>? Attachments = null);

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default);

    Task SendTemplatedEmailAsync(EmailDispatchRequest request, CancellationToken ct = default);
}
