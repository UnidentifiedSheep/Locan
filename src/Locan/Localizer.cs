using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.Interfaces.Rendering;

namespace Locan;

public class Localizer : ILocalizer
{
	private readonly ILocalizerContainerProvider _containerProvider;
	private readonly IMessageTemplateRenderer _renderer;

	public Localizer(
		ILocalizerContainerProvider containerProvider,
		IMessageTemplateRenderer renderer)
	{
		ArgumentNullException.ThrowIfNull(containerProvider);
		ArgumentNullException.ThrowIfNull(renderer);

		_containerProvider = containerProvider;
		_renderer = renderer;
	}

	public string Get(ILocalizableMessage message, CultureInfo locale)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(locale);

		var container = _containerProvider.GetRequired(locale);

		return !container.TryGetValue(message.MessageKey, out var template)
			? throw new MessageTemplateNotFoundException(message.MessageKey)
			: _renderer.Render(template, message);
	}

	public bool TryGet(
		ILocalizableMessage message,
		CultureInfo locale,
		[NotNullWhen(true)] out string? value)
	{
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(locale);

		var container = _containerProvider.Find(locale);
		if (container != null && container.TryGetValue(message.MessageKey, out var segmentsContainer))
			return _renderer.TryRender(
				segmentsContainer,
				message,
				out value);

		value = null;
		return false;
	}

	public bool IsSupported(CultureInfo locale)
		=> _containerProvider.Find(locale) != null;
}
