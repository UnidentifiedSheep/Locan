using System;
using System.Collections.Generic;
using Locan.Core.Enums;
using Locan.Core.Parsing;
using Locan.Core.Segments;
using Locan.Generator.Models;

namespace Locan.Generator.Parsing;

internal static class PlaceholderParser
{
	private static readonly MessageSegmentType[] AllowedSegmentTypes =
	[
		MessageSegmentType.Placeholder,
		MessageSegmentType.Type,
		MessageSegmentType.Format
	];

	public static IReadOnlyDictionary<string, PlaceholderInfo> Parse(string template)
	{
		var segments = MessageTemplateParser.Parse(template, AllowedSegmentTypes);
		var placeholders = new Dictionary<string, PlaceholderInfo>(StringComparer.Ordinal);
		PlaceholderInfo? current = null;

		foreach (var segment in segments)
			switch (segment)
			{
				case PlaceholderMessageSegment placeholder:
					if (!placeholders.TryGetValue(placeholder.Value, out current))
					{
						current = new PlaceholderInfo(placeholder.Value);
						placeholders.Add(placeholder.Value, current);
					}
					break;

				case TypeMessageSegment type when current is not null:
					current.Type = type.Value;
					break;

				case FormatMessageSegment format when current is not null:
					current.Format = format.Value;
					break;
			}

		return placeholders;
	}

	public static bool AreEquivalent(
		IReadOnlyDictionary<string, PlaceholderInfo> left,
		IReadOnlyDictionary<string, PlaceholderInfo> right)
	{
		if (left.Count != right.Count) return false;

		foreach (var pair in left)
			if (!right.TryGetValue(pair.Key, out var value) ||
				!string.Equals(pair.Value.Type, value.Type, StringComparison.Ordinal) ||
				!string.Equals(pair.Value.Format, value.Format, StringComparison.Ordinal))
				return false;

		return true;
	}
}
