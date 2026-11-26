// Date: 2026-11-24
// Purpose: Configuration options capping per-window self check-ins to mitigate relayed rotating QR/check-in codes.

using System.ComponentModel.DataAnnotations;

namespace TeamUp.Application.Common.Security;

public sealed class AttendanceOptions
{
    public const string SectionName = "Attendance";

    /// <summary>
    /// Maximum self check-ins accepted for one session within one rotating-code window. The code is shown to a room, so a
    /// burst far above the real attendance rate signals the code being relayed to people who are not present.
    /// </summary>
    [Range(1, 10000)]
    public int MaxSelfCheckInsPerWindow { get; set; } = 120;

    /// <summary>
    /// Minutes before a session's start time when check-in opens. Checking in long before a session proves nothing
    /// about being in the room, so attendance recorded outside the window is rejected.
    /// </summary>
    [Range(0, 1440)]
    public int EarlyCheckInMinutes { get; set; } = 30;

    /// <summary>
    /// Minutes after a session's end time when check-in closes, so organizers can record stragglers right after the session.
    /// </summary>
    [Range(0, 1440)]
    public int LateCheckInMinutes { get; set; } = 15;
}
