using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Locan.Core.Interfaces.Localizers;

public interface ILocalizer
{
	string Get(
		ILocalizableMessage message,
		CultureInfo culture);

	bool TryGet(
		ILocalizableMessage message,
		CultureInfo culture,
		[NotNullWhen(true)]
		out string? value);

	bool IsSupported(CultureInfo culture);
}
