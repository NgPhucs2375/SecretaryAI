

namespace Qeq.Application.Interfaces;

public interface IQeqTool
{
    string Name { get; }
    string Description { get; }
    string ParametersJson { get; }
    Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct);
}