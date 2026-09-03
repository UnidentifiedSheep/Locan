namespace Locan.Core.Interfaces.Initialization;

public interface ILocalizerInitializer
{
	Task InitializeAsync(
		string directoryPath,
		string searchPattern = "*.json",
		bool recursive = true,
		CancellationToken cancellationToken = default);
}
