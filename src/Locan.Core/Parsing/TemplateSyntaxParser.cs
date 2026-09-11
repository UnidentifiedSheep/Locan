using System.Text;
using Locan.Core.Exceptions;
using Locan.Core.Parsing.Syntax;

namespace Locan.Core.Parsing;

internal static class TemplateSyntaxParser
{
	public static IReadOnlyList<TemplateSyntax> Parse(string template)
	{
		var reader = new TemplateReader(template);
		List<TemplateSyntax> syntax = [];
		var text = new StringBuilder();
		var textPosition = 0;

		while (!reader.End)
		{
			if (reader.TryConsume('{', '{'))
			{
				StartText(text, reader.Position - 2, ref textPosition);
				text.Append('{');
				continue;
			}

			if (reader.TryConsume('}', '}'))
			{
				StartText(text, reader.Position - 2, ref textPosition);
				text.Append('}');
				continue;
			}

			if (reader.Current == '{')
			{
				FlushText(syntax, text, textPosition);
				syntax.Add(ParsePlaceholder(ref reader));
				continue;
			}

			if (reader.Current == '}')
				throw Error(reader.Position, "Unexpected closing brace");

			StartText(text, reader.Position, ref textPosition);
			text.Append(reader.ReadUntilBrace().ToString());
		}

		FlushText(syntax, text, textPosition);
		return syntax;
	}

	private static PlaceholderSyntax ParsePlaceholder(ref TemplateReader reader)
	{
		var openingPosition = reader.Position;
		reader.Advance();
		var contentPosition = reader.Position;

		while (!reader.End && reader.Current != '}')
		{
			if (reader.Current == '{')
				throw Error(reader.Position, "Nested placeholder");

			reader.Advance();
		}

		if (reader.End)
			throw Error(openingPosition, "Unclosed placeholder");

		var content = reader.Slice(contentPosition, reader.Position);
		reader.Advance();
		return PlaceholderSyntaxParser.Parse(content, openingPosition, contentPosition);
	}

	private static void StartText(
		StringBuilder text,
		int position,
		ref int textPosition)
	{
		if (text.Length == 0)
			textPosition = position;
	}

	private static void FlushText(
		ICollection<TemplateSyntax> syntax,
		StringBuilder text,
		int position)
	{
		if (text.Length == 0)
			return;

		syntax.Add(new TextSyntax(position, text.ToString()));
		text.Clear();
	}

	private static MessageTemplateParseException Error(int position, string reason) =>
		new(position, reason);
}
