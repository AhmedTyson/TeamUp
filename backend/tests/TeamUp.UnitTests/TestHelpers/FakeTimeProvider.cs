// Date: 2026-11-27
// Purpose: Deterministic controllable TimeProvider for instant time-dependent unit testing without sleeps.

namespace TeamUp.UnitTests.TestHelpers;

public sealed class FakeTimeProvider(DateTimeOffset initialUtcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = initialUtcNow.ToUniversalTime();

    public FakeTimeProvider()
        : this(new DateTimeOffset(2026, 11, 27, 12, 0, 0, TimeSpan.Zero))
    {
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan delta) => _utcNow = _utcNow.Add(delta);

    public void SetUtcNow(DateTimeOffset utcNow) => _utcNow = utcNow.ToUniversalTime();
}
