using Locan.Core.Exceptions;
using Locan.Core.Parsing.Syntax;

namespace Locan.Core.Parsing;

internal static class PlaceholderSyntaxParser
{
	public static PlaceholderSyntax Parse(
		ReadOnlySpan<char> content,
		int openingPosition,
		int contentPosition)
	{
		var parts = content.ToString().Split('|');

		if (parts.Length > 3)
			throw Error(openingPosition, "Placeholder has too many parts");

		var key = parts[0];

		if (!IsIdentifier(key))
			throw Error(contentPosition, "Invalid placeholder key");

		string? type = null;
		string? format = null;

		if (parts.Length >= 2)
		{
			type = parts[1];
			var typePosition = contentPosition + key.Length + 1;

			if (!IsIdentifier(type))
				throw Error(typePosition, "Invalid placeholder type");
		}

		if (parts.Length == 3)
		{
			format = parts[2];
			var formatPosition = contentPosition + key.Length + type!.Length + 2;

			if (!IsFormat(format))
				throw Error(formatPosition, "Invalid placeholder format");
		}

		return new PlaceholderSyntax(openingPosition, key, type, format);
	}

	private static bool IsIdentifier(string value)
	{
		if (value.Length == 0 || !IsIdentifierStart(value[0]))
			return false;

		for (var index = 1; index < value.Length; index++)
		{
			if (!IsIdentifierPart(value[index]))
				return false;
		}

		return true;
	}

	private static bool IsIdentifierStart(char value) =>
		value is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_';

	private static bool IsIdentifierPart(char value) =>
		IsIdentifierStart(value) || value is >= '0' and <= '9';

	private static bool IsFormat(string value)
	{
		if (value.Length == 0 || char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length - 1]))
			return false;

		for (var index = 0; index < value.Length; index++)
		{
			if (char.IsWhiteSpace(value[index]) && value[index] != ' ')
				return false;
		}

		return true;
	}

	private static MessageTemplateParseException Error(int position, string reason) =>
		new(position, reason);
}
