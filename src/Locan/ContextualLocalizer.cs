using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Localizers;

namespace Locan;

public sealed class ContextualLocalizer : IContextualLocalizer
{
	private readonly ILocalizer _localizer;

	public ContextualLocalizer(ILocalizer localizer)
	{
		ArgumentNullException.ThrowIfNull(localizer);
		_localizer = localizer;
	}

	public string Get(ILocalizableMessage message)
	{
		ArgumentNullException.ThrowIfNull(message);
		return _localizer.Get(message, CultureInfo.CurrentUICulture);
	}

	public bool TryGet(
		ILocalizableMessage message,
		[NotNullWhen(true)] out string? value)
	{
		ArgumentNullException.ThrowIfNull(message);
		return _localizer.TryGet(message, CultureInfo.CurrentUICulture, out value);
	}
}
