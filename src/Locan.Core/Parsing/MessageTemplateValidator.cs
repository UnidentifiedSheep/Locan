using Locan.Core.Exceptions;
using Locan.Core.Parsing.Syntax;

namespace Locan.Core.Parsing;

internal static class MessageTemplateValidator
{
	public static void Validate(IEnumerable<TemplateSyntax> syntax)
	{
		var placeholders = new Dictionary<string, PlaceholderMetadata>(StringComparer.Ordinal);

		foreach (var placeholder in syntax.OfType<PlaceholderSyntax>())
		{
			var metadata = new PlaceholderMetadata(placeholder.ValueType, placeholder.Format);

			if (placeholders.TryGetValue(placeholder.Key, out var existing) && existing != metadata)
				throw new MessageTemplateParseException(
					placeholder.Position,
					$"Conflicting metadata for placeholder '{placeholder.Key}'");

			placeholders[placeholder.Key] = metadata;
		}
	}

	private readonly record struct PlaceholderMetadata(string? Type, string? Format);
}
