using Locan.Core.Enums;
using Locan.Core.Segments;

namespace Locan.Core.Parsing;

public static class MessageTemplateParser
{
	public static IReadOnlyList<MessageSegment> Parse(
		string template,
		params MessageSegmentType[] allowed)
	{
		ArgumentNullException.ThrowIfNull(template);
		ArgumentNullException.ThrowIfNull(allowed);

		List<MessageSegment> segments = [];
		var textStart = 0;

		for (var i = 0; i < template.Length; i++)
		{
			if (template[i] != '{')
				continue;

			Add(
				segments,
				MessageSegment.Text(template[textStart..i]),
				allowed,
				skipEmpty: true);

			var endIndex = template.IndexOf('}', i + 1);

			if (endIndex < 0)
				throw new FormatException($"Unclosed placeholder in template '{template}'.");

			var nestedIndex = template.IndexOf('{', i + 1, endIndex - i - 1);

			if (nestedIndex >= 0)
				throw new FormatException($"Nested placeholder in template '{template}'.");

			var parts = template[(i + 1)..endIndex].Split('|');

			if (parts.Length > 3)
				throw new FormatException($"Placeholder has too many parts in template '{template}'.");

			if (string.IsNullOrWhiteSpace(parts[0]))
				throw new FormatException($"Empty placeholder in template '{template}'.");

			Add(segments, MessageSegment.Placeholder(parts[0]), allowed);

			if (parts.Length >= 2)
			{
				if (string.IsNullOrWhiteSpace(parts[1]))
					throw new FormatException($"Empty placeholder type in template '{template}'.");

				Add(segments, MessageSegment.Type(parts[1]), allowed);
			}

			if (parts.Length == 3)
			{
				if (string.IsNullOrWhiteSpace(parts[2]))
					throw new FormatException($"Empty placeholder format in template '{template}'.");

				Add(segments, MessageSegment.Format(parts[2]), allowed);
			}

			i = endIndex;
			textStart = endIndex + 1;
		}

		Add(
			segments,
			MessageSegment.Text(template[textStart..]),
			allowed,
			skipEmpty: true);

		return segments;
	}

	private static void Add(
		List<MessageSegment> segments,
		MessageSegment segment,
		MessageSegmentType[] allowed,
		bool skipEmpty = false)
	{
		if (skipEmpty && segment.Value.Length == 0)
			return;

		if (allowed.Length == 0 || allowed.Contains(segment.Kind))
			segments.Add(segment);
	}
}
