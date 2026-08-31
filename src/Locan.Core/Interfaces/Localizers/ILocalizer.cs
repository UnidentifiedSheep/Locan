using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Locan.Core.Interfaces.Localizers;

public interface ILocalizer
{
	string Get(
		ILocalizableMessage message,
		CultureInfo locale);

	bool TryGet(
		ILocalizableMessage message,
		CultureInfo locale,
		[NotNullWhen(true)]
		out string? value);

	bool IsSupported(CultureInfo locale);
}
