using System.Collections.Frozen;
using System.Globalization;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces.Containers;

namespace Locan.Containers;

public sealed class LocalizerContainerProvider :
	ILocalizerContainerProvider,
	ILocalizerContainerRegistry
{
	private FrozenDictionary<string, ILocalizerContainer> _containers =
		FrozenDictionary<string, ILocalizerContainer>.Empty;

	public void SetContainers(IEnumerable<ILocalizerContainer> containers)
	{
		ArgumentNullException.ThrowIfNull(containers);

		var next = new Dictionary<string, ILocalizerContainer>(StringComparer.OrdinalIgnoreCase);

		foreach (var container in containers)
		{
			ArgumentNullException.ThrowIfNull(container);

			if (!next.TryAdd(container.Locale.Name, container))
				throw new ArgumentException(
					$"A localizer container for '{container.Locale.Name}' is already registered.",
					nameof(containers));
		}

		var snapshot = next.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
		Volatile.Write(ref _containers, snapshot);
	}

	public ILocalizerContainer? Find(CultureInfo culture)
	{
		ArgumentNullException.ThrowIfNull(culture);

		var containers = Volatile.Read(ref _containers);
		var current = culture;

		while (!string.IsNullOrEmpty(current.Name))
		{
			if (containers.TryGetValue(current.Name, out var container))
				return container;

			current = current.Parent;
		}

		return null;
	}

	public ILocalizerContainer GetRequired(CultureInfo culture) =>
		Find(culture) ?? throw new LocalizerContainerNotFoundException(culture);
}
