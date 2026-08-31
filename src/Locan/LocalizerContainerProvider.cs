using System.Globalization;
using Locan.Core.Interfaces.Containers;

namespace Locan;

public sealed class LocalizerContainerProvider : ILocalizerContainerProvider
{
	private readonly Dictionary<string, ILocalizerContainer> _containers;

	public LocalizerContainerProvider(
		IEnumerable<ILocalizerContainer> containers)
	{
		ArgumentNullException.ThrowIfNull(containers);

		_containers = containers.ToDictionary(
			x => x.Locale.Name,
			StringComparer.OrdinalIgnoreCase);
	}

	public ILocalizerContainer? Find(CultureInfo culture)
	{
		ArgumentNullException.ThrowIfNull(culture);
		return _containers.GetValueOrDefault(culture.Name);
	}

	public ILocalizerContainer? TryGetRequired(CultureInfo culture)
	{
		ArgumentNullException.ThrowIfNull(culture);

		var current = culture;

		while (!string.IsNullOrEmpty(current.Name))
		{
			if (_containers.TryGetValue(current.Name, out var container))
				return container;

			current = current.Parent;
		}

		return null;
	}
}
