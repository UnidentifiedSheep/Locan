using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;

namespace Locan.Tests;

public sealed class LocalizerContainerProviderTests
{
	[Fact]
	public void Find_BeforePublicationReturnsNull()
	{
		var provider = new LocalizerContainerProvider();

		var result = provider.Find(CultureInfo.GetCultureInfo("en"));

		Assert.Null(result);
	}

	[Fact]
	public void Find_ReturnsExactContainer()
	{
		var expected = CreateContainer("en-US");
		var provider = CreateProvider(expected);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(expected, actual);
	}

	[Fact]
	public void Find_PrefersExactContainerOverParent()
	{
		var parent = CreateContainer("en");
		var exact = CreateContainer("en-US");
		var provider = CreateProvider(parent, exact);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(exact, actual);
	}

	[Fact]
	public void Find_FallsBackToParentCulture()
	{
		var expected = CreateContainer("en");
		var provider = CreateProvider(expected);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(expected, actual);
	}

	[Fact]
	public void GetRequired_ThrowsWhenCultureIsNotSupported()
	{
		var provider = new LocalizerContainerProvider();
		var culture = CultureInfo.GetCultureInfo("en-US");

		Assert.Throws<LocalizerContainerNotFoundException>(
			() => provider.GetRequired(culture));
	}

	[Fact]
	public void SetContainers_ReplacesPreviousSnapshot()
	{
		var english = CreateContainer("en");
		var german = CreateContainer("de");
		var provider = CreateProvider(english);

		provider.SetContainers([german]);

		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Same(german, provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public void SetContainers_DoesNotRetainMutableSource()
	{
		var english = CreateContainer("en");
		var german = CreateContainer("de");
		var source = new List<SegmentedLocalizerContainer> { english };
		var provider = new LocalizerContainerProvider();
		provider.SetContainers(source);

		source.Add(german);

		Assert.Same(english, provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public void SetContainers_FailedPublicationPreservesPreviousSnapshot()
	{
		var english = CreateContainer("en");
		var firstGerman = CreateContainer("de");
		var secondGerman = CreateContainer("de");
		var provider = CreateProvider(english);

		Assert.Throws<ArgumentException>(
			() => provider.SetContainers([firstGerman, secondGerman]));

		Assert.Same(english, provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public void SetContainers_RejectsNullCollection()
	{
		var provider = new LocalizerContainerProvider();

		Assert.Throws<ArgumentNullException>(() => provider.SetContainers(null!));
	}

	[Fact]
	public void SetContainers_RejectsNullContainerAndPreservesPreviousSnapshot()
	{
		var english = CreateContainer("en");
		var provider = CreateProvider(english);

		Assert.Throws<ArgumentNullException>(
			() => provider.SetContainers([CreateContainer("de"), null!]));

		Assert.Same(english, provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	private static LocalizerContainerProvider CreateProvider(
		params SegmentedLocalizerContainer[] containers)
	{
		var provider = new LocalizerContainerProvider();
		provider.SetContainers(containers);
		return provider;
	}

	private static SegmentedLocalizerContainer CreateContainer(string locale)
		=> new(
			CultureInfo.GetCultureInfo(locale),
			new Dictionary<string, string>());
}
