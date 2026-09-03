using Locan.Core.Exceptions;
using Locan.LocalizableMessages;
using Locan.TemplateRenderers;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class SegmentedMessageTemplateRendererTests
{
	private readonly SegmentedMessageTemplateRenderer _renderer = new();

	[Fact]
	public void Render_ReturnsTextWithoutPlaceholders()
	{
		var template = TestFactory.CreateTemplate("plain text");
		var message = new LocalizableMessage("Message");

		var result = _renderer.Render(template, message);

		Assert.Equal("plain text", result);
	}

	[Fact]
	public void Render_ReplacesMultiplePlaceholders()
	{
		var template = TestFactory.CreateTemplate("{Greeting}, {Name}!");
		var message = new LocalizableMessage("Message")
			.WithValue("Greeting", "Hello")
			.WithValue("Name", "Alex");

		var result = _renderer.Render(template, message);

		Assert.Equal("Hello, Alex!", result);
	}

	[Fact]
	public void Render_ReplacesRepeatedPlaceholder()
	{
		var template = TestFactory.CreateTemplate("{Value}-{Value}");
		var message = new LocalizableMessage("Message").WithValue("Value", "A");

		var result = _renderer.Render(template, message);

		Assert.Equal("A-A", result);
	}

	[Fact]
	public void Render_RendersNullValueAsEmptyText()
	{
		var template = TestFactory.CreateTemplate("before{Value}after");
		var message = new LocalizableMessage("Message").WithValue("Value", null);

		var result = _renderer.Render(template, message);

		Assert.Equal("beforeafter", result);
	}

	[Fact]
	public void Render_ThrowsSpecificExceptionForMissingValue()
	{
		var template = TestFactory.CreateTemplate("{Missing}");
		var message = new LocalizableMessage("Message");

		var exception = Assert.Throws<PlaceholderValueNotFoundException>(
			() => _renderer.Render(template, message));

		Assert.Equal("Missing", exception.PlaceholderKey);
	}

	[Fact]
	public void TryRender_ReturnsRenderedValue()
	{
		var template = TestFactory.CreateTemplate("Hello {Name}");
		var message = new LocalizableMessage("Message").WithValue("Name", "Alex");

		var result = _renderer.TryRender(template, message, out var rendered);

		Assert.True(result);
		Assert.Equal("Hello Alex", rendered);
	}

	[Fact]
	public void TryRender_ReturnsFalseForMissingValue()
	{
		var template = TestFactory.CreateTemplate("{Missing}");
		var message = new LocalizableMessage("Message");

		var result = _renderer.TryRender(template, message, out var rendered);

		Assert.False(result);
		Assert.Null(rendered);
	}

	[Fact]
	public void Render_RejectsNullTemplate()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_renderer.Render(null!, new LocalizableMessage("Message")));
	}

	[Fact]
	public void Render_RejectsNullMessage()
	{
		Assert.Throws<ArgumentNullException>(() =>
			_renderer.Render(TestFactory.CreateTemplate("text"), null!));
	}
}
