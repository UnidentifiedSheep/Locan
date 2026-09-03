using Locan.Core.Interfaces.Initialization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Locan.Hosting;

public sealed class LocalizerInitializationHostedService : IHostedService
{
	private readonly ILocalizerInitializer _initializer;
	private readonly IOptions<LocanOptions> _options;

	public LocalizerInitializationHostedService(
		ILocalizerInitializer initializer,
		IOptions<LocanOptions> options)
	{
		ArgumentNullException.ThrowIfNull(initializer);
		ArgumentNullException.ThrowIfNull(options);

		_initializer = initializer;
		_options = options;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var options = _options.Value;

		return _initializer.InitializeAsync(
			options.DirectoryPath,
			options.SearchPattern,
			options.Recursive,
			cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
