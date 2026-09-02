using System.Globalization;
using Locan.Containers;
using Locan.Core.Segments;

namespace Locan.Tests;

public sealed class SegmentedLocalizerContainerTests
{
	[Fact]
	public void Initialize_StoresOnlyRuntimeSegments()
	{
		var container = CreateContainer();

		container.Initialize(new Dictionary<string, string>
		{
			["Value"] = "Date: {Date|DateTime|yyyy-MM-dd}"
		});

		Assert.Collection(
			container["Value"],
			segment => Assert.IsType<TextMessageSegment>(segment),
			segment => Assert.IsType<PlaceholderMessageSegment>(segment));
	}

	[Fact]
	public void Initialize_CanOnlyBeCalledOnce()
	{
		var container = CreateContainer();
		container.Initialize(new Dictionary<string, string>());

		Assert.Throws<InvalidOperationException>(
			() => container.Initialize(new Dictionary<string, string>()));
	}

	[Fact]
	public void Initialize_DoesNotPublishPartialState()
	{
		var container = CreateContainer();

		Assert.Throws<FormatException>(() => container.Initialize(
			new Dictionary<string, string>
			{
				["Valid"] = "Valid",
				["Invalid"] = "{Value"
			}));

		Assert.Throws<InvalidOperationException>(() => _ = container.Count);

		container.Initialize(new Dictionary<string, string>
		{
			["Valid"] = "Valid"
		});

		Assert.Single(container);
	}

	[Fact]
	public void ReadBeforeInitialize_Throws()
	{
		var container = CreateContainer();

		Assert.Throws<InvalidOperationException>(() => _ = container.Count);
	}

	private static SegmentedLocalizerContainer CreateContainer() =>
		new(CultureInfo.GetCultureInfo("en"));
}
