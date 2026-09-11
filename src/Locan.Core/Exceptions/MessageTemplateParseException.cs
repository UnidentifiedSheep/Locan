using Locan.Core.Compatibility;

namespace Locan.Core.Exceptions;

public sealed class MessageTemplateParseException : FormatException
{
	public MessageTemplateParseException(int position, string reason)
		: base(CreateMessage(position, reason))
	{
		Position = position;
		Reason = reason;
	}

	public int Position { get; }

	public string Reason { get; }

	private static string CreateMessage(int position, string reason)
	{
		if (position < 0)
			throw new ArgumentOutOfRangeException(nameof(position));
		ArgumentGuard.NotNullOrWhiteSpace(reason, nameof(reason));
		return $"{reason} at position {position}.";
	}
}
