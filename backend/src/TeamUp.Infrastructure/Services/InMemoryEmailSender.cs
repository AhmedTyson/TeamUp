// Date: 2026-11-21
// Purpose: Development/testing-only email sink with bounded 15-minute TTL expiration and zero token logging (STRIDE THR-S-03).

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Infrastructure.Services;

public class InMemoryEmailSender(ILogger<InMemoryEmailSender> logger) : IEmailSender
{
    private static readonly TimeSpan TokenRetentionWindow = TimeSpan.FromMinutes(15);
    private readonly ConcurrentDictionary<string, (string Token, DateTime ExpiresAtUtc)> _lastResetTokens =
        new(StringComparer.OrdinalIgnoreCase);

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        PurgeExpired();
        _lastResetTokens[toEmail] = (resetToken, DateTime.UtcNow.Add(TokenRetentionWindow));
        logger.LogInformation("Dispatched password reset message via development sink to {Email}", toEmail);
        return Task.CompletedTask;
    }

    public string? GetLastResetToken(string email)
    {
        PurgeExpired();
        if (_lastResetTokens.TryGetValue(email, out var entry) && entry.ExpiresAtUtc > DateTime.UtcNow)
            return entry.Token;

        _lastResetTokens.TryRemove(email, out _);
        return null;
    }

    public void Clear() => _lastResetTokens.Clear();

    private void PurgeExpired()
    {
        var now = DateTime.UtcNow;
        foreach (var kvp in _lastResetTokens)
        {
            if (kvp.Value.ExpiresAtUtc <= now)
                _lastResetTokens.TryRemove(kvp.Key, out _);
        }
    }
}
