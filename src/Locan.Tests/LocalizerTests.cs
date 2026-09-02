using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;
using Locan.LocalizableMessages;
using Locan.TemplateRenderers;

namespace Locan.Tests;

public sealed class LocalizerTests
{
	[Fact]
	public void Get_UsesParentCultureFallback()
	{
		var container = CreateContainer("en", "Hello {Name|String}");
		var localizer = CreateLocalizer(container);
		var message = new LocalizableMessage("Greeting").WithValue("Name", "Alex");

		var result = localizer.Get(message, CultureInfo.GetCultureInfo("en-US"));

		Assert.Equal("Hello Alex", result);
	}

	[Fact]
	public void Get_ThrowsSpecificExceptionForMissingTemplate()
	{
		var container = CreateContainer("en", "Hello");
		var localizer = CreateLocalizer(container);
		var message = new LocalizableMessage("Missing");

		Assert.Throws<MessageTemplateNotFoundException>(
			() => localizer.Get(message, CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public void TryGet_ReturnsFalseForMissingPlaceholderValue()
	{
		var container = CreateContainer("en", "Hello {Name|String}");
		var localizer = CreateLocalizer(container);
		var message = new LocalizableMessage("Greeting");

		var result = localizer.TryGet(
			message,
			CultureInfo.GetCultureInfo("en"),
			out var value);

		Assert.False(result);
		Assert.Null(value);
	}

	private static global::Locan.Localizer CreateLocalizer(
		SegmentedLocalizerContainer container) =>
		new(
			new LocalizerContainerProvider([container]),
			new SegmentedMessageTemplateRenderer());

	private static SegmentedLocalizerContainer CreateContainer(
		string locale,
		string template)
	{
		var container = new SegmentedLocalizerContainer(CultureInfo.GetCultureInfo(locale));
		container.Initialize(new Dictionary<string, string>
		{
			["Greeting"] = template
		});
		return container;
	}

}
