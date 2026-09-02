using System.Globalization;
using Locan.Containers;
using Locan.Core.Segments;

namespace Locan.Tests;

public sealed class SegmentedLocalizerContainerTests
{
	[Fact]
	public void Constructor_StoresOnlyRuntimeSegments()
	{
		var container = CreateContainer(new Dictionary<string, string>
		{
			["Value"] = "Date: {Date|DateTime|yyyy-MM-dd}"
		});

		Assert.Collection(
			container["Value"],
			segment => Assert.IsType<TextMessageSegment>(segment),
			segment => Assert.IsType<PlaceholderMessageSegment>(segment));
	}

	[Fact]
	public void Constructor_CreatesReadyEmptyContainer()
	{
		var container = CreateContainer(new Dictionary<string, string>());

		Assert.Empty(container);
	}

	[Fact]
	public void Constructor_DoesNotRetainMutableSource()
	{
		var messages = new Dictionary<string, string>
		{
			["First"] = "First"
		};
		var container = CreateContainer(messages);

		messages["Second"] = "Second";

		Assert.Single(container);
		Assert.False(container.ContainsKey("Second"));
	}

	[Fact]
	public void Constructor_RejectsInvalidTemplate()
	{
		Assert.Throws<FormatException>(() => CreateContainer(
			new Dictionary<string, string>
			{
				["Invalid"] = "{Value"
			}));
	}

	[Fact]
	public void Constructor_RejectsEmptyMessageKey()
	{
		Assert.Throws<ArgumentException>(() => CreateContainer(
			new Dictionary<string, string>
			{
				[""] = "Value"
			}));
	}

	[Fact]
	public void Constructor_RejectsNullLocale()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new SegmentedLocalizerContainer(
				null!,
				new Dictionary<string, string>()));
	}

	[Fact]
	public void Constructor_RejectsNullMessages()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new SegmentedLocalizerContainer(
				CultureInfo.GetCultureInfo("en"),
				null!));
	}

	private static SegmentedLocalizerContainer CreateContainer(
		IReadOnlyDictionary<string, string> messages) =>
		new(CultureInfo.GetCultureInfo("en"), messages);
}
