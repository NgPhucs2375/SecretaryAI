using Qeq.Application.Interfaces;

namespace Qeq.Application.Tests.Fakes;

public class FakeTool : IQeqTool
{
    private readonly Func<string, string> _execute;

    public string Name { get; }
    public string Description => "Tool giả để test";
    public string ParametersJson => """{"type":"object","properties":{}}""";

    public List<string> Received { get; } = new();

    public FakeTool(string name, Func<string, string> execute)
    {
        Name = name;
        _execute = execute;
    }

    public Task<string> ExecuteAsync(string argumentsJson, CancellationToken ct)
    {
        Received.Add(argumentsJson);
        return Task.FromResult(_execute(argumentsJson));
    }
}