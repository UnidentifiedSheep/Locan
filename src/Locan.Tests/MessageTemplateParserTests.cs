using Locan.Core.Enums;
using Locan.Core.Exceptions;
using Locan.Core.Parsing;
using Locan.Core.Segments;

namespace Locan.Tests;

public sealed class MessageTemplateParserTests
{
	[Fact]
	public void Parse_EmptyTemplateReturnsNoSegments()
	{
		var segments = MessageTemplateParser.Parse(string.Empty);

		Assert.Empty(segments);
	}

	[Fact]
	public void Parse_TextOnlyReturnsTextSegment()
	{
		var segments = MessageTemplateParser.Parse("plain text");

		AssertSegments(segments, MessageSegment.Text("plain text"));
	}

	[Fact]
	public void Parse_PlaceholderOnlyDoesNotAddEmptyTextSegments()
	{
		var segments = MessageTemplateParser.Parse("{Name}");

		AssertSegments(segments, MessageSegment.Placeholder("Name"));
	}

	[Fact]
	public void Parse_PlaceholderWithTypeReturnsTwoSegments()
	{
		var segments = MessageTemplateParser.Parse("{Value|String}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("Value"),
			MessageSegment.Type("String"));
	}

	[Fact]
	public void Parse_PlaceholderWithTypeAndFormatReturnsThreeSegments()
	{
		var segments = MessageTemplateParser.Parse("{Price|Decimal|F2}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("Price"),
			MessageSegment.Type("Decimal"),
			MessageSegment.Format("F2"));
	}

	[Fact]
	public void Parse_ReturnsAllSegmentKinds()
	{
		var segments = MessageTemplateParser.Parse(
			"some values {Date|DateTime|yyyy-MM-dd} other {Price|Decimal|F2}");

		AssertSegments(
			segments,
			MessageSegment.Text("some values "),
			MessageSegment.Placeholder("Date"),
			MessageSegment.Type("DateTime"),
			MessageSegment.Format("yyyy-MM-dd"),
			MessageSegment.Text(" other "),
			MessageSegment.Placeholder("Price"),
			MessageSegment.Type("Decimal"),
			MessageSegment.Format("F2"));
	}

	[Fact]
	public void Parse_PreservesAdjacentPlaceholders()
	{
		var segments = MessageTemplateParser.Parse("{First}{Second}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("First"),
			MessageSegment.Placeholder("Second"));
	}

	[Fact]
	public void Parse_PreservesTextBetweenPlaceholders()
	{
		var segments = MessageTemplateParser.Parse("{First} middle {Second}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("First"),
			MessageSegment.Text(" middle "),
			MessageSegment.Placeholder("Second"));
	}

	[Fact]
	public void Parse_PreservesUnicodeValues()
	{
		var segments = MessageTemplateParser.Parse("Привет, {Name|String}! 👋");

		AssertSegments(
			segments,
			MessageSegment.Text("Привет, "),
			MessageSegment.Placeholder("Name"),
			MessageSegment.Type("String"),
			MessageSegment.Text("! 👋"));
	}

	[Fact]
	public void Parse_AllowsSpacesInsideFormat()
	{
		var segments = MessageTemplateParser.Parse("{Date|DateTime|yyyy MM dd}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("Date"),
			MessageSegment.Type("DateTime"),
			MessageSegment.Format("yyyy MM dd"));
	}

	[Fact]
	public void Parse_AllowsCustomIdentifierType()
	{
		var segments = MessageTemplateParser.Parse("{Value|CustomType}");

		AssertSegments(
			segments,
			MessageSegment.Placeholder("Value"),
			MessageSegment.Type("CustomType"));
	}

	[Fact]
	public void Parse_UnescapesBracesInText()
	{
		var segments = MessageTemplateParser.Parse("Object: {{ Name: {Name} }}");

		AssertSegments(
			segments,
			MessageSegment.Text("Object: { Name: "),
			MessageSegment.Placeholder("Name"),
			MessageSegment.Text(" }"));
	}

	[Fact]
	public void Parse_EscapedPlaceholderSyntaxRemainsText()
	{
		var segments = MessageTemplateParser.Parse("{{Value}}");

		AssertSegments(segments, MessageSegment.Text("{Value}"));
	}

	[Fact]
	public void Parse_AllowsRepeatedPlaceholderWithSameMetadata()
	{
		var segments = MessageTemplateParser.Parse(
			"{Price|Decimal|F2} and {Price|Decimal|F2}");

		Assert.Equal(7, segments.Count);
		Assert.Equal("Price", segments[0].Value);
		Assert.Equal("Price", segments[4].Value);
	}

	[Fact]
	public void Parse_TreatsPlaceholderKeysAsCaseSensitive()
	{
		var segments = MessageTemplateParser.Parse("{Value|String}{value|Decimal}");

		Assert.Equal(4, segments.Count);
	}

	[Fact]
	public void Parse_FiltersMultipleSegmentKinds()
	{
		var segments = MessageTemplateParser.Parse(
			"Value: {Price|Decimal|F2}",
			MessageSegmentType.Text,
			MessageSegmentType.Placeholder);

		AssertSegments(
			segments,
			MessageSegment.Text("Value: "),
			MessageSegment.Placeholder("Price"));
	}

	[Fact]
	public void Parse_TextFilterReturnsEveryTextPart()
	{
		var segments = MessageTemplateParser.Parse(
			"before{Value|Decimal|F2}after",
			MessageSegmentType.Text);

		AssertSegments(
			segments,
			MessageSegment.Text("before"),
			MessageSegment.Text("after"));
	}

	[Theory]
	[InlineData(MessageSegmentType.Placeholder, "Value")]
	[InlineData(MessageSegmentType.Type, "Decimal")]
	[InlineData(MessageSegmentType.Format, "F2")]
	public void Parse_SingleFilterReturnsRequestedSegment(
		MessageSegmentType allowed,
		string expectedValue)
	{
		var segments = MessageTemplateParser.Parse(
			"{Value|Decimal|F2}",
			allowed);

		var segment = Assert.Single(segments);
		Assert.Equal(allowed, segment.Kind);
		Assert.Equal(expectedValue, segment.Value);
	}

	[Fact]
	public void Parse_FilterWithoutMatchingSegmentsReturnsEmptyCollection()
	{
		var segments = MessageTemplateParser.Parse(
			"plain text",
			MessageSegmentType.Placeholder);

		Assert.Empty(segments);
	}

	[Fact]
	public void Parse_DuplicateAllowedKindsDoNotDuplicateSegments()
	{
		var segments = MessageTemplateParser.Parse(
			"{Value}",
			MessageSegmentType.Placeholder,
			MessageSegmentType.Placeholder);

		AssertSegments(segments, MessageSegment.Placeholder("Value"));
	}

	[Fact]
	public void Parse_ValidatesFilteredOutSegments()
	{
		Assert.Throws<MessageTemplateParseException>(() => MessageTemplateParser.Parse(
			"text {Value||F2}",
			MessageSegmentType.Text));
	}

	[Fact]
	public void Parse_RejectsConflictingPlaceholderType()
	{
		Assert.Throws<MessageTemplateParseException>(() => MessageTemplateParser.Parse(
			"{Value|String} {Value|Decimal}"));
	}

	[Fact]
	public void Parse_RejectsConflictingPlaceholderFormat()
	{
		Assert.Throws<MessageTemplateParseException>(() => MessageTemplateParser.Parse(
			"{Value|Decimal|F2} {Value|Decimal|F4}"));
	}

	[Fact]
	public void Parse_ErrorContainsPositionAndReason()
	{
		var exception = Assert.Throws<MessageTemplateParseException>(
			() => MessageTemplateParser.Parse("abc }"));

		Assert.Equal(4, exception.Position);
		Assert.Equal("Unexpected closing brace", exception.Reason);
		Assert.Equal("Unexpected closing brace at position 4.", exception.Message);
	}

	[Fact]
	public void Parse_NullTemplateThrows()
		=> Assert.Throws<ArgumentNullException>(() => MessageTemplateParser.Parse(null!));

	[Fact]
	public void Parse_NullAllowedKindsThrows()
	{
		Assert.Throws<ArgumentNullException>(
			() => MessageTemplateParser.Parse("text", null!));
	}

	[Theory]
	[InlineData("{")]
	[InlineData("prefix {Value")]
	[InlineData("{}")]
	[InlineData("{ }")]
	[InlineData("{|String}")]
	[InlineData("{Value|")]
	[InlineData("{Value|}")]
	[InlineData("{Value| }")]
	[InlineData("{Value||F2}")]
	[InlineData("{Value|Decimal|}")]
	[InlineData("{Value|Decimal| }")]
	[InlineData("{Value|Decimal|F2|extra}")]
	[InlineData("{ Value}")]
	[InlineData("{Value }")]
	[InlineData("{Value |String}")]
	[InlineData("{Value| String}")]
	[InlineData("{Value|String }")]
	[InlineData("{Value|String| F2}")]
	[InlineData("{Value|String|F2 }")]
	[InlineData("{Value|String|F\t2}")]
	[InlineData("{Value|String|F\n2}")]
	[InlineData("{User-Name}")]
	[InlineData("{User.Name}")]
	[InlineData("{1Value}")]
	[InlineData("{Имя}")]
	[InlineData("{Value|System.DateTime}")]
	[InlineData("text }")]
	public void Parse_RejectsInvalidTemplate(string template)
		=> Assert.Throws<MessageTemplateParseException>(() => MessageTemplateParser.Parse(template));

	private static void AssertSegments(
		IReadOnlyList<MessageSegment> actual,
		params MessageSegment[] expected) =>
		Assert.Equal(expected, actual);
}
