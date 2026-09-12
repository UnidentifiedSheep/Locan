using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Locan.Core.Enums;
using Locan.Core.Exceptions;
using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Rendering;

namespace Locan.TemplateRenderers;

public sealed class SegmentedMessageTemplateRenderer : IMessageTemplateRenderer
{
	public bool TryRender(
		IMessageSegmentsContainer segmentsContainer,
		ILocalizableMessage message,
		CultureInfo culture,
		[NotNullWhen(true)] out string? rendered)
	{
		return RenderCore(
			segmentsContainer,
			message,
			culture,
			out rendered,
			out _);
	}

	public string Render(
		IMessageSegmentsContainer segmentsContainer,
		ILocalizableMessage message,
		CultureInfo culture)
	{
		return RenderCore(
			segmentsContainer,
			message,
			culture,
			out var rendered,
			out var missingKey)
			? rendered
			: throw new PlaceholderValueNotFoundException(missingKey!);
	}

	private static bool RenderCore(
		IMessageSegmentsContainer segmentsContainer,
		ILocalizableMessage message,
		CultureInfo culture,
		[NotNullWhen(true)] out string? rendered,
		out string? missingKey)
	{
		ArgumentNullException.ThrowIfNull(segmentsContainer);
		ArgumentNullException.ThrowIfNull(message);
		ArgumentNullException.ThrowIfNull(culture);

		missingKey = null;

		var valuesLength = 0;
		var formattedValues = new Dictionary<string, string?>(StringComparer.Ordinal);

		for (var i = 0; i < segmentsContainer.Count; i++)
		{
			var segment = segmentsContainer[i];

			if (segment.Kind != MessageSegmentType.Placeholder) continue;

			if (formattedValues.TryGetValue(segment.Value, out var formattedValue))
			{
				valuesLength += formattedValue?.Length ?? 0;
				continue;
			}

			if (!message.Values.TryGetValue(segment.Value, out var value))
			{
				rendered = null;
				missingKey = segment.Value;
				return false;
			}

			formattedValue = FormatValue(value.Value, value.Format, culture);
			formattedValues.Add(segment.Value, formattedValue);
			valuesLength += formattedValue?.Length ?? 0;
		}

		rendered = string.Create(
			length: segmentsContainer.TextSegmentsTotalLength + valuesLength,
			state: (segmentsContainer, formattedValues),
			action: static (destination, state) =>
			{
				var (segments, values) = state;
				var offset = 0;

				for (var i = 0; i < segments.Count; i++)
				{
					var segment = segments[i];

					offset = segment.Kind switch
					{
						MessageSegmentType.Text => RenderText(
							destination,
							offset,
							segment.Value),

						MessageSegmentType.Placeholder => RenderPlaceholder(
							destination,
							offset,
							values[segment.Value]),

						_ => throw new InvalidOperationException($"Unknown segment type '{segment.Kind}'.")
					};
				}
			});

		return true;
	}

	private static int RenderText(
		Span<char> destination,
		int offset,
		string text)
	{
		text.AsSpan().CopyTo(destination[offset..]);
		return offset + text.Length;
	}

	private static int RenderPlaceholder(
		Span<char> destination,
		int offset,
		string? value)
	{
		if (value is null) return offset;

		value.AsSpan().CopyTo(destination[offset..]);
		return offset + value.Length;
	}

	private static string? FormatValue(
		object? value,
		string? format,
		CultureInfo culture)
	{
		if (value is null)
			return null;

		return value is IFormattable formattable
			? formattable.ToString(format, culture)
			: value.ToString();
	}
}
