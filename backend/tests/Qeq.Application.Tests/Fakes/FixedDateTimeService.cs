using Qeq.Application.Interfaces;

namespace Qeq.Application.Tests.Fakes;

public class FixedDateTimeService : IDateTimeService
{
    public DateTime NowUtc => new(2026, 11, 19, 1, 0, 0, DateTimeKind.Utc);
    public DateTime Now => new(2026, 11, 19, 8, 0, 0, DateTimeKind.Local);
}