using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.Interfaces.Rendering;

namespace Locan;

public class Localizer(
	ILocalizerContainerProvider containerProvider,
	IMessageTemplateRenderer renderer) : ILocalizer
{
	public string Get(ILocalizableMessage message, CultureInfo locale)
	{
		var container = containerProvider.Find(locale) ?? throw new LocalizerContainerNotFound(locale);
		var template = container[message.MessageKey];
		return renderer.Render(template, message);
	}

	public bool TryGet(
		ILocalizableMessage message,
		CultureInfo locale,
		[NotNullWhen(true)] out string? value)
	{
		var container = containerProvider.Find(locale);
		if (container != null && container.TryGetValue(message.MessageKey, out var segmentsContainer))
			return renderer.TryRender(
				segmentsContainer,
				message,
				out value);

		value = null;
		return false;
	}

	public bool IsSupported(CultureInfo locale)
		=> containerProvider.TryGetRequired(locale) != null;
}
