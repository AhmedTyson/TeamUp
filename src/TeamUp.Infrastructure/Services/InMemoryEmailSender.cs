using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Infrastructure.Services;

public class InMemoryEmailSender(ILogger<InMemoryEmailSender> logger) : IEmailSender
{
    private readonly ConcurrentDictionary<string, string> _lastResetTokens = new(StringComparer.OrdinalIgnoreCase);

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        _lastResetTokens[toEmail] = resetToken;
        logger.LogInformation("Dispatched password reset token to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public string? GetLastResetToken(string email)
    {
        _lastResetTokens.TryGetValue(email, out var token);
        return token;
    }
}
