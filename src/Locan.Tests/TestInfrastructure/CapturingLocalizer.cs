using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;

namespace Locan.Tests.TestInfrastructure;

internal sealed class CapturingLocalizer : ILocalizer
{
	public CultureInfo? Culture { get; private set; }

	public string Get(ILocalizableMessage message, CultureInfo culture)
	{
		Culture = culture;
		return culture.Name;
	}

	public bool TryGet(
		ILocalizableMessage message,
		CultureInfo culture,
		[NotNullWhen(true)] out string? value)
	{
		Culture = culture;
		value = culture.Name;
		return true;
	}

	public bool IsSupported(CultureInfo culture) => true;
}
