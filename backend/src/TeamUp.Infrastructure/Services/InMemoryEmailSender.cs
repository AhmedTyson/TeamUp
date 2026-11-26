// Date: 2026-11-21
// Purpose: Development/testing-only email sink with bounded 15-minute TTL expiration and zero token logging (STRIDE THR-S-03).

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using TeamUp.Application.Common.Interfaces;

namespace TeamUp.Infrastructure.Services;

public sealed record RecordedEmailAttachment(
    string FileName,
    string ContentType,
    int SizeBytes);

public sealed record RecordedEmailDispatch(
    string ToEmail,
    string Subject,
    string HtmlBody,
    string? PlainTextBody,
    IReadOnlyList<RecordedEmailAttachment> Attachments,
    DateTime SentAtUtc);

public sealed partial class InMemoryEmailSender(ILogger<InMemoryEmailSender> logger, TimeProvider? timeProvider = null) : IEmailSender
{
    private const int MaxRecordedMessages = 100;
    private static readonly TimeSpan TokenRetentionWindow = TimeSpan.FromMinutes(15);

    [LoggerMessage(Level = LogLevel.Information, Message = "Dispatched password reset message via development sink to {Email}")]
    private static partial void LogPasswordResetDispatched(ILogger logger, string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Recorded templated email '{Subject}' with {AttachmentCount} attachment(s) via development sink to {Email}")]
    private static partial void LogTemplatedEmailRecorded(ILogger logger, string subject, int attachmentCount, string email);

    private readonly TimeProvider _time = timeProvider ?? TimeProvider.System;
    private readonly ConcurrentDictionary<string, (string Token, DateTime ExpiresAtUtc)> _lastResetTokens =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<RecordedEmailDispatch> _sentMessages = new();

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken ct = default)
    {
        PurgeExpired();
        _lastResetTokens[toEmail] = (resetToken, _time.GetUtcNow().UtcDateTime.Add(TokenRetentionWindow));
        LogPasswordResetDispatched(logger, toEmail);
        return Task.CompletedTask;
    }

    public Task SendTemplatedEmailAsync(EmailDispatchRequest request, CancellationToken ct = default)
    {
        var attachments = (request.Attachments ?? [])
            .Select(a => new RecordedEmailAttachment(a.FileName, a.ContentType, a.Content.Length))
            .ToArray();

        _sentMessages.Enqueue(new RecordedEmailDispatch(
            request.ToEmail,
            request.Subject,
            request.HtmlBody,
            request.PlainTextBody,
            attachments,
            _time.GetUtcNow().UtcDateTime));

        while (_sentMessages.Count > MaxRecordedMessages)
            _sentMessages.TryDequeue(out _);

        LogTemplatedEmailRecorded(logger, request.Subject, attachments.Length, request.ToEmail);

        return Task.CompletedTask;
    }

    public IReadOnlyList<RecordedEmailDispatch> GetSentMessages(string? toEmail = null)
    {
        var snapshot = _sentMessages.ToArray();
        if (string.IsNullOrWhiteSpace(toEmail))
            return snapshot;

        var normalized = toEmail.Trim();
        return snapshot
            .Where(m => string.Equals(m.ToEmail, normalized, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    public string? GetLastResetToken(string email)
    {
        PurgeExpired();
        if (_lastResetTokens.TryGetValue(email, out var entry) && entry.ExpiresAtUtc > _time.GetUtcNow().UtcDateTime)
            return entry.Token;

        _lastResetTokens.TryRemove(email, out _);
        return null;
    }

    public void Clear()
    {
        _lastResetTokens.Clear();
        while (_sentMessages.TryDequeue(out _))
        {
        }
    }

    private void PurgeExpired()
    {
        var now = _time.GetUtcNow().UtcDateTime;
        foreach (var kvp in _lastResetTokens)
        {
            if (kvp.Value.ExpiresAtUtc <= now)
                _lastResetTokens.TryRemove(kvp.Key, out _);
        }
    }
}
