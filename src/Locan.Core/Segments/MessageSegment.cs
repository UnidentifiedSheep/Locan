using Locan.Core.Enums;

namespace Locan.Core.Segments;

public abstract record MessageSegment
{
	public string Value { get; }
	public abstract MessageSegmentType Kind { get; }

	protected MessageSegment(string value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));
		Value = value;
	}

	public static TextMessageSegment Text(string text) => new(text);

	public static PlaceholderMessageSegment Placeholder(string key) => new(key);

	public static TypeMessageSegment Type(string type) => new(type);

	public static FormatMessageSegment Format(string format) => new(format);
}

public sealed record TextMessageSegment : MessageSegment
{
	public TextMessageSegment(string value) : base(value)
	{
	}

	public override MessageSegmentType Kind => MessageSegmentType.Text;
}

public sealed record PlaceholderMessageSegment : MessageSegment
{
	public PlaceholderMessageSegment(string value) : base(value)
	{
	}

	public override MessageSegmentType Kind => MessageSegmentType.Placeholder;
}

public sealed record TypeMessageSegment : MessageSegment
{
	public TypeMessageSegment(string value) : base(value)
	{
	}

	public override MessageSegmentType Kind => MessageSegmentType.Type;
}

public sealed record FormatMessageSegment : MessageSegment
{
	public FormatMessageSegment(string value) : base(value)
	{
	}

	public override MessageSegmentType Kind => MessageSegmentType.Format;
}
