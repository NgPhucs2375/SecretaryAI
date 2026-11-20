namespace Qeq.Application.Interfaces;

public interface IToolRegistry
{
    IReadOnlyCollection<IQeqTool> All { get; }
    IQeqTool? Find(string name);
}