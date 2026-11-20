using Qeq.Application.Interfaces;

namespace Qeq.Application.Tools;

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, IQeqTool> _tools;

    public ToolRegistry(IEnumerable<IQeqTool> tools)
    {
        _tools = tools.ToDictionary(t => t.Name);
    }

    public IReadOnlyCollection<IQeqTool> All => _tools.Values;

    public IQeqTool? Find(string name) => _tools.GetValueOrDefault(name);
}