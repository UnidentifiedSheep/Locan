using System.Globalization;

namespace Locan.Core.Exceptions;

public sealed class LocalizerContainerNotFound : Exception
{
	public LocalizerContainerNotFound(CultureInfo cultureInfo)
		: base($"Localizer container for {cultureInfo} not found")
	{
		Data.Add(nameof(cultureInfo), cultureInfo);
	}
}
