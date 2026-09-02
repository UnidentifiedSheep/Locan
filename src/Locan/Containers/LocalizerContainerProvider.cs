using System.Globalization;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces.Containers;

namespace Locan.Containers;

public sealed class LocalizerContainerProvider : ILocalizerContainerProvider
{
	private readonly Dictionary<string, ILocalizerContainer> _containers;

	public LocalizerContainerProvider(
		IEnumerable<ILocalizerContainer> containers)
	{
		ArgumentNullException.ThrowIfNull(containers);

		_containers = new Dictionary<string, ILocalizerContainer>(StringComparer.OrdinalIgnoreCase);

		foreach (var container in containers)
		{
			ArgumentNullException.ThrowIfNull(container);

			if (!_containers.TryAdd(container.Locale.Name, container))
				throw new ArgumentException(
					$"A localizer container for '{container.Locale.Name}' is already registered.",
					nameof(containers));
		}
	}

	public ILocalizerContainer? Find(CultureInfo culture)
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

	public ILocalizerContainer GetRequired(CultureInfo culture) =>
		Find(culture) ?? throw new LocalizerContainerNotFoundException(culture);
}
