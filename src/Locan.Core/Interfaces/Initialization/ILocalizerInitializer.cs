namespace Locan.Core.Interfaces.Initialization;

public interface ILocalizerInitializer
{
	Task InitializeAsync(
		string directoryPath,
		bool recursive = true,
		CancellationToken cancellationToken = default);
}
