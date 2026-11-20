namespace Qeq.Application.Interfaces;

public interface IDateTimeService
{
    DateTime NowUtc { get; }
    DateTime Now { get; }
}