using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;
using Locan.Tests.TestInfrastructure;

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
		var expected = TestFactory.CreateContainer("en-US");
		var provider = TestFactory.CreateProvider(expected);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(expected, actual);
	}

	[Fact]
	public void Find_PrefersExactContainerOverParent()
	{
		var parent = TestFactory.CreateContainer();
		var exact = TestFactory.CreateContainer("en-US");
		var provider = TestFactory.CreateProvider(parent, exact);

		var actual = provider.Find(CultureInfo.GetCultureInfo("en-US"));

		Assert.Same(exact, actual);
	}

	[Fact]
	public void Find_FallsBackToParentCulture()
	{
		var expected = TestFactory.CreateContainer();
		var provider = TestFactory.CreateProvider(expected);

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
		var english = TestFactory.CreateContainer();
		var german = TestFactory.CreateContainer("de");
		var provider = TestFactory.CreateProvider(english);

		provider.SetContainers([german]);

		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Same(german, provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public void SetContainers_DoesNotRetainMutableSource()
	{
		var english = TestFactory.CreateContainer();
		var german = TestFactory.CreateContainer("de");
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
		var english = TestFactory.CreateContainer();
		var firstGerman = TestFactory.CreateContainer("de");
		var secondGerman = TestFactory.CreateContainer("de");
		var provider = TestFactory.CreateProvider(english);

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
		var english = TestFactory.CreateContainer();
		var provider = TestFactory.CreateProvider(english);

		Assert.Throws<ArgumentNullException>(
			() => provider.SetContainers([TestFactory.CreateContainer("de"), null!]));

		Assert.Same(english, provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("de")));
	}

}
