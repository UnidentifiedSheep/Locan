using System.Globalization;
using Locan.Core.Exceptions;
using Locan.LocalizableMessages;
using Locan.TemplateRenderers;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class SegmentedMessageTemplateRendererTests
{
	private readonly SegmentedMessageTemplateRenderer _renderer = new();
	private readonly CultureInfo _culture = CultureInfo.InvariantCulture;

	[Fact]
	public void Render_ReturnsTextWithoutPlaceholders()
	{
		var template = TestFactory.CreateTemplate("plain text");
		var message = new LocalizableMessage("Message");

		var result = _renderer.Render(template, message, _culture);

		Assert.Equal("plain text", result);
	}

	[Fact]
	public void Render_ReplacesMultiplePlaceholders()
	{
		var template = TestFactory.CreateTemplate("{Greeting}, {Name}!");
		var message = new LocalizableMessage("Message")
			.WithValue("Greeting", "Hello")
			.WithValue("Name", "Alex");

		var result = _renderer.Render(template, message, _culture);

		Assert.Equal("Hello, Alex!", result);
	}

	[Fact]
	public void Render_ReplacesRepeatedPlaceholder()
	{
		var template = TestFactory.CreateTemplate("{Value}-{Value}");
		var message = new LocalizableMessage("Message").WithValue("Value", "A");

		var result = _renderer.Render(template, message, _culture);

		Assert.Equal("A-A", result);
	}

	[Fact]
	public void Render_RendersNullValueAsEmptyText()
	{
		var template = TestFactory.CreateTemplate("before{Value}after");
		var message = new LocalizableMessage("Message").WithValue("Value", null);

		var result = _renderer.Render(template, message, _culture);

		Assert.Equal("beforeafter", result);
	}

	[Fact]
	public void Render_ThrowsSpecificExceptionForMissingValue()
	{
		var template = TestFactory.CreateTemplate("{Missing}");
		var message = new LocalizableMessage("Message");

		var exception = Assert.Throws<PlaceholderValueNotFoundException>(
			() => _renderer.Render(template, message, _culture));

		Assert.Equal("Missing", exception.PlaceholderKey);
	}

	[Fact]
	public void TryRender_ReturnsRenderedValue()
	{
		var template = TestFactory.CreateTemplate("Hello {Name}");
		var message = new LocalizableMessage("Message").WithValue("Name", "Alex");

		var result = _renderer.TryRender(template, message, _culture, out var rendered);

		Assert.True(result);
		Assert.Equal("Hello Alex", rendered);
	}

	[Fact]
	public void TryRender_ReturnsFalseForMissingValue()
	{
		var template = TestFactory.CreateTemplate("{Missing}");
		var message = new LocalizableMessage("Message");

		var result = _renderer.TryRender(template, message, _culture, out var rendered);

		Assert.False(result);
		Assert.Null(rendered);
	}

	[Fact]
	public void Render_RejectsNullTemplate()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_renderer.Render(null!, new LocalizableMessage("Message"), _culture));
	}

	[Fact]
	public void Render_RejectsNullMessage()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_renderer.Render(TestFactory.CreateTemplate("text"), null!, _culture));
	}

	[Fact]
	public void Render_FormatsValueUsingSpecifiedCulture()
	{
		var template = TestFactory.CreateTemplate("Value: {Value}");
		var message = new LocalizableMessage("Message")
			.WithValue("Value", 1234.5m, "N2");
		var culture = CultureInfo.GetCultureInfo("fr-FR");

		var result = _renderer.Render(template, message, culture);

		Assert.Equal($"Value: {1234.5m.ToString("N2", culture)}", result);
	}

	[Fact]
	public void Render_RejectsNullCulture()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_renderer.Render(
				TestFactory.CreateTemplate("text"),
				new LocalizableMessage("Message"),
				null!));
	}
}
