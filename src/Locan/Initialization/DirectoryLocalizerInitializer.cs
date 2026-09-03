using System.Globalization;
using Locan.Containers;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Initialization;

namespace Locan.Initialization;

public sealed class DirectoryLocalizerInitializer : ILocalizerInitializer
{
	private readonly ILocalizerContainerRegistry _registry;

	public DirectoryLocalizerInitializer(ILocalizerContainerRegistry registry)
	{
		ArgumentNullException.ThrowIfNull(registry);
		_registry = registry;
	}

	public async Task InitializeAsync(
		string directoryPath,
		bool recursive = true,
		CancellationToken cancellationToken = default)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
		cancellationToken.ThrowIfCancellationRequested();

		var fullPath = Path.GetFullPath(directoryPath);

		if (!Directory.Exists(fullPath))
			throw new DirectoryNotFoundException(
				$"Localization directory '{fullPath}' was not found.");

		var files = Directory
			.EnumerateFiles(
				fullPath,
				"*.json",
				recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
			.Order(StringComparer.Ordinal)
			.ToArray();

		var bundles = new Dictionary<string, CultureBundle>(StringComparer.OrdinalIgnoreCase);

		foreach (var file in files)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var (culture, messages) = await LocalizationFileReader.ReadAsync(file, cancellationToken);

			if (!bundles.TryGetValue(culture.Name, out var bundle))
			{
				bundle = new CultureBundle(culture);
				bundles.Add(culture.Name, bundle);
			}

			foreach (var (key, template) in messages)
				bundle.Messages.Add(key, template);
		}

		var containers = bundles.Values
			.Select(bundle => new SegmentedLocalizerContainer(bundle.Culture, bundle.Messages))
			.ToArray();

		cancellationToken.ThrowIfCancellationRequested();
		_registry.SetContainers(containers);
	}

	private sealed class CultureBundle(CultureInfo culture)
	{
		public CultureInfo Culture { get; } = culture;

		public Dictionary<string, string> Messages { get; } = new(StringComparer.Ordinal);
	}
}
