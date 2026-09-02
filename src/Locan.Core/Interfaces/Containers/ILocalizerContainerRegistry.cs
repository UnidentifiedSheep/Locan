namespace Locan.Core.Interfaces.Containers;

public interface ILocalizerContainerRegistry
{
	void SetContainers(IEnumerable<ILocalizerContainer> containers);
}
