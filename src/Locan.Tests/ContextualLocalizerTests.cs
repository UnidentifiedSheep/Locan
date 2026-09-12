using Locan.Core.LocalizableMessages;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class ContextualLocalizerTests
{
	private readonly CapturingLocalizer _inner = new();

	[Fact]
	public void Get_UsesCurrentUICulture()
	{
		var localizer = new ContextualLocalizer(_inner);
		using var scope = new CurrentUiCultureScope("fr-FR");

		var result = localizer.Get(new LocalizableMessage("Message"));

		Assert.Equal("fr-FR", result);
		Assert.Equal("fr-FR", _inner.Culture?.Name);
	}

	[Fact]
	public void TryGet_UsesCurrentUICulture()
	{
		var localizer = new ContextualLocalizer(_inner);
		using var scope = new CurrentUiCultureScope("de-DE");

		var result = localizer.TryGet(
			new LocalizableMessage("Message"),
			out var value);

		Assert.True(result);
		Assert.Equal("de-DE", value);
		Assert.Equal("de-DE", _inner.Culture?.Name);
	}

	[Fact]
	public void Constructor_RejectsNullLocalizer()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new ContextualLocalizer(null!));
	}

	[Fact]
	public void Get_RejectsNullMessage()
	{
		var localizer = new ContextualLocalizer(_inner);

		Assert.Throws<ArgumentNullException>(() => localizer.Get(null!));
	}
}
