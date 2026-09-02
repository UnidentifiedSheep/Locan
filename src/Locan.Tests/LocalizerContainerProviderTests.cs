using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;

namespace Locan.Tests;

public sealed class LocalizerContainerProviderTests
{
	[Fact]
	public void Find_ReturnsExactContainer()
	{
		var expected = CreateContainer("en-US");
		var provider = new LocalizerContainerProvider([expected]);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(expected, actual);
	}

	[Fact]
	public void Find_FallsBackToParentCulture()
	{
		var expected = CreateContainer("en");
		var provider = new LocalizerContainerProvider([expected]);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(expected, actual);
	}

	[Fact]
	public void GetRequired_ThrowsWhenCultureIsNotSupported()
	{
		var provider = new LocalizerContainerProvider([]);
		var culture = CultureInfo.GetCultureInfo("en-US");

		Assert.Throws<LocalizerContainerNotFoundException>(
			() => provider.GetRequired(culture));
	}

	[Fact]
	public void Constructor_RejectsDuplicateLocales()
	{
		var first = CreateContainer("en");
		var second = CreateContainer("en");

		Assert.Throws<ArgumentException>(
			() => new LocalizerContainerProvider([first, second]));
	}

	private static SegmentedLocalizerContainer CreateContainer(string locale)
	{
		var container = new SegmentedLocalizerContainer(CultureInfo.GetCultureInfo(locale));
		container.Initialize(new Dictionary<string, string>());
		return container;
	}
}
