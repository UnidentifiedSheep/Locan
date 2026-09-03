using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;
using Locan.Core.Segments;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class SegmentedLocalizerContainerTests
{
	[Fact]
	public void Constructor_StoresOnlyRuntimeSegments()
	{
		var container = TestFactory.CreateContainer(messages: new Dictionary<string, string>
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
		var container = TestFactory.CreateContainer();

		Assert.Empty(container);
	}

	[Fact]
	public void Constructor_DoesNotRetainMutableSource()
	{
		var messages = new Dictionary<string, string>
		{
			["First"] = "First"
		};
		var container = TestFactory.CreateContainer(messages: messages);

		messages["Second"] = "Second";

		Assert.Single(container);
		Assert.False(container.ContainsKey("Second"));
	}

	[Fact]
	public void Constructor_RejectsInvalidTemplate()
	{
		Assert.Throws<MessageTemplateParseException>(() => TestFactory.CreateContainer(
			messages: new Dictionary<string, string>
			{
				["Invalid"] = "{Value"
			}));
	}

	[Fact]
	public void Constructor_RejectsEmptyMessageKey()
	{
		Assert.Throws<ArgumentException>(() => TestFactory.CreateContainer(
			messages: new Dictionary<string, string>
			{
				[""] = "Value"
			}));
	}

	[Fact]
	public void Constructor_RejectsNullCulture()
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
}
