// Date: 2026-11-25
// Purpose: Configuration options for notification inbox sizing, so the page size the API serves by default and the
// most a caller may request are tunable per deployment instead of compiled in.

using System.ComponentModel.DataAnnotations;

namespace TeamUp.Application.Common.Security;

public sealed class NotificationOptions
{
    public const string SectionName = "Notifications";

    /// <summary>Default number of notifications returned when the caller does not pass a take value.</summary>
    [Range(1, 500)]
    public int DefaultInboxTake { get; set; } = 15;

    /// <summary>Maximum number of notifications a single inbox request may return.</summary>
    [Range(1, 1000)]
    public int MaxInboxTake { get; set; } = 100;
}
