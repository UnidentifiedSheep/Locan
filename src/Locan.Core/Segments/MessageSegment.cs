using Locan.Core.Enums;

namespace Locan.Core.Segments;

public readonly record struct MessageSegment
{
	public MessageSegmentType Type { get; }
	public string Value { get; }

	private MessageSegment(
		MessageSegmentType type,
		string value)
	{
		Type = type;
		Value = value;
	}

	public static MessageSegment Text(string text)
		=> new(MessageSegmentType.Text, text);

	public static MessageSegment Placeholder(string key)
		=> new(MessageSegmentType.Placeholder, key);
}
