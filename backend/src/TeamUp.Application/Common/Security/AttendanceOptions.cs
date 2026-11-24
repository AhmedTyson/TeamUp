// Date: 2026-11-24
// Purpose: Configuration options capping per-window self check-ins to mitigate relayed rotating QR/check-in codes.

namespace TeamUp.Application.Common.Security;

public sealed class AttendanceOptions
{
    public const string SectionName = "Attendance";

    /// <summary>
    /// Maximum self check-ins accepted for one session within one rotating-code window. The code is shown to a room, so a
    /// burst far above the real attendance rate signals the code being relayed to people who are not present.
    /// </summary>
    public int MaxSelfCheckInsPerWindow { get; set; } = 120;
}
