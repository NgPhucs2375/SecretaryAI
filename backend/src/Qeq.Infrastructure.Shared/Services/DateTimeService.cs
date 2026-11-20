using Qeq.Application.Interfaces;

namespace Qeq.Infrastructure.Shared.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime NowUtc => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}