using Locan.Core.Enums;
using Locan.Core.Parsing.Syntax;
using Locan.Core.Segments;

namespace Locan.Core.Parsing;

internal static class MessageSegmentProjector
{
	public static IReadOnlyList<MessageSegment> Project(
		IEnumerable<TemplateSyntax> syntax,
		MessageSegmentType[] allowed)
	{
		List<MessageSegment> segments = [];

		foreach (var node in syntax)
			switch (node)
			{
				case TextSyntax text:
					Add(segments, MessageSegment.Text(text.Value), allowed);
					break;

				case PlaceholderSyntax placeholder:
					Add(segments, MessageSegment.Placeholder(placeholder.Key), allowed);

					if (placeholder.ValueType is not null)
						Add(segments, MessageSegment.Type(placeholder.ValueType), allowed);

					if (placeholder.Format is not null)
						Add(segments, MessageSegment.Format(placeholder.Format), allowed);

					break;

				default:
					throw new InvalidOperationException(
						$"Unsupported template syntax '{node.GetType().Name}'.");
			}

		return segments;
	}

	private static void Add(
		List<MessageSegment> segments,
		MessageSegment segment,
		MessageSegmentType[] allowed)
	{
		if (allowed.Length == 0 || allowed.Contains(segment.Kind))
			segments.Add(segment);
	}
}
