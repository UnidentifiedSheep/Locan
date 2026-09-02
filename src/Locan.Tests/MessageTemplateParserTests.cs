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

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "plain text"));
	}

	[Fact]
	public void Parse_PlaceholderOnlyDoesNotAddEmptyTextSegments()
	{
		var segments = MessageTemplateParser.Parse("{Name}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Name"));
	}

	[Fact]
	public void Parse_PlaceholderWithTypeReturnsTwoSegments()
	{
		var segments = MessageTemplateParser.Parse("{Value|String}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Value"),
			segment => AssertSegment<TypeMessageSegment>(segment, "String"));
	}

	[Fact]
	public void Parse_PlaceholderWithTypeAndFormatReturnsThreeSegments()
	{
		var segments = MessageTemplateParser.Parse("{Price|Decimal|F2}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Price"),
			segment => AssertSegment<TypeMessageSegment>(segment, "Decimal"),
			segment => AssertSegment<FormatMessageSegment>(segment, "F2"));
	}

	[Fact]
	public void Parse_ReturnsAllSegmentKinds()
	{
		var segments = MessageTemplateParser.Parse(
			"some values {Date|DateTime|yyyy-MM-dd} other {Price|Decimal|F2}");

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "some values "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Date"),
			segment => AssertSegment<TypeMessageSegment>(segment, "DateTime"),
			segment => AssertSegment<FormatMessageSegment>(segment, "yyyy-MM-dd"),
			segment => AssertSegment<TextMessageSegment>(segment, " other "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Price"),
			segment => AssertSegment<TypeMessageSegment>(segment, "Decimal"),
			segment => AssertSegment<FormatMessageSegment>(segment, "F2"));
	}

	[Fact]
	public void Parse_PreservesAdjacentPlaceholders()
	{
		var segments = MessageTemplateParser.Parse("{First}{Second}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "First"),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Second"));
	}

	[Fact]
	public void Parse_PreservesTextBetweenPlaceholders()
	{
		var segments = MessageTemplateParser.Parse("{First} middle {Second}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "First"),
			segment => AssertSegment<TextMessageSegment>(segment, " middle "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Second"));
	}

	[Fact]
	public void Parse_PreservesUnicodeValues()
	{
		var segments = MessageTemplateParser.Parse("Привет, {Name|String}! 👋");

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "Привет, "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Name"),
			segment => AssertSegment<TypeMessageSegment>(segment, "String"),
			segment => AssertSegment<TextMessageSegment>(segment, "! 👋"));
	}

	[Fact]
	public void Parse_AllowsSpacesInsideFormat()
	{
		var segments = MessageTemplateParser.Parse("{Date|DateTime|yyyy MM dd}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Date"),
			segment => AssertSegment<TypeMessageSegment>(segment, "DateTime"),
			segment => AssertSegment<FormatMessageSegment>(segment, "yyyy MM dd"));
	}

	[Fact]
	public void Parse_AllowsCustomIdentifierType()
	{
		var segments = MessageTemplateParser.Parse("{Value|CustomType}");

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Value"),
			segment => AssertSegment<TypeMessageSegment>(segment, "CustomType"));
	}

	[Fact]
	public void Parse_UnescapesBracesInText()
	{
		var segments = MessageTemplateParser.Parse("Object: {{ Name: {Name} }}");

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "Object: { Name: "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Name"),
			segment => AssertSegment<TextMessageSegment>(segment, " }"));
	}

	[Fact]
	public void Parse_EscapedPlaceholderSyntaxRemainsText()
	{
		var segments = MessageTemplateParser.Parse("{{Value}}");

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "{Value}"));
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

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "Value: "),
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Price"));
	}

	[Fact]
	public void Parse_TextFilterReturnsEveryTextPart()
	{
		var segments = MessageTemplateParser.Parse(
			"before{Value|Decimal|F2}after",
			MessageSegmentType.Text);

		Assert.Collection(
			segments,
			segment => AssertSegment<TextMessageSegment>(segment, "before"),
			segment => AssertSegment<TextMessageSegment>(segment, "after"));
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

		Assert.Collection(
			segments,
			segment => AssertSegment<PlaceholderMessageSegment>(segment, "Value"));
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

	private static void AssertSegment<TSegment>(MessageSegment segment, string value)
		where TSegment : MessageSegment
	{
		var typed = Assert.IsType<TSegment>(segment);
		Assert.Equal(value, typed.Value);
	}
}
