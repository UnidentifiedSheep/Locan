using System.Globalization;

namespace Locan.Core.Exceptions;

public sealed class LocalizerContainerNotFoundException : Exception
{
	public LocalizerContainerNotFoundException(CultureInfo culture)
		: base($"Localizer container for '{culture?.Name ?? throw new ArgumentNullException(nameof(culture))}' was not found.")
	{
		Data.Add(nameof(culture), culture);
	}
}
