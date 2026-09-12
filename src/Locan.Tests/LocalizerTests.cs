using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;
using Locan.Core.LocalizableMessages;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class LocalizerTests
{
	[Fact]
	public void Get_UsesParentCultureFallback()
	{
		var container = CreateContainer("en", "Hello {Name|String}");
		var localizer = TestFactory.CreateLocalizer(container);
		var message = new LocalizableMessage("Greeting").WithValue("Name", "Alex");

		var result = localizer.Get(message, CultureInfo.GetCultureInfo("en-US"));

		Assert.Equal("Hello Alex", result);
	}

	[Fact]
	public void Get_ThrowsSpecificExceptionForMissingTemplate()
	{
		var container = CreateContainer("en", "Hello");
		var localizer = TestFactory.CreateLocalizer(container);
		var message = new LocalizableMessage("Missing");

		Assert.Throws<MessageTemplateNotFoundException>(
			() => localizer.Get(message, CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public void TryGet_ReturnsFalseForMissingPlaceholderValue()
	{
		var container = CreateContainer("en", "Hello {Name|String}");
		var localizer = TestFactory.CreateLocalizer(container);
		var message = new LocalizableMessage("Greeting");

		var result = localizer.TryGet(
			message,
			CultureInfo.GetCultureInfo("en"),
			out var value);

		Assert.False(result);
		Assert.Null(value);
	}

	[Fact]
	public void Get_FormatsValuesUsingRequestedCulture()
	{
		var culture = CultureInfo.GetCultureInfo("fr-FR");
		var container = CreateContainer(culture.Name, "Value: {Value}");
		var localizer = TestFactory.CreateLocalizer(container);
		var message = new LocalizableMessage("Greeting")
			.WithValue("Value", 1234.5m, "N2");

		var result = localizer.Get(message, culture);

		Assert.Equal($"Value: {1234.5m.ToString("N2", culture)}", result);
	}

	private static SegmentedLocalizerContainer CreateContainer(
		string culture,
		string template) => TestFactory.CreateContainer(
			culture,
			new Dictionary<string, string>
			{
				["Greeting"] = template
			});

}
