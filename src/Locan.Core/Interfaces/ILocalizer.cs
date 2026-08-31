using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Models;

namespace Locan.Core.Interfaces;

public interface ILocalizer
{
	string Get(
		LocalizableMessage message,
		CultureInfo locale);

	bool TryGet(
		LocalizableMessage message,
		CultureInfo locale,
		[NotNullWhen(true)]
		out string? value);

	bool IsSupported(CultureInfo locale);
}
