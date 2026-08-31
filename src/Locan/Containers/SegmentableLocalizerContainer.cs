using System.Collections;
using System.Globalization;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Segments;

namespace Locan.Containers;

public sealed class SegmentableLocalizerContainer : ILocalizerContainer
{
	private readonly Dictionary<string, IMessageSegmentsContainer> _containers = [];

	public SegmentableLocalizerContainer(CultureInfo locale)
	{
		ArgumentNullException.ThrowIfNull(locale);

		Locale = locale;
	}

	public CultureInfo Locale { get; }

	public int Count => _containers.Count;

	public IEnumerable<string> Keys => _containers.Keys;

	public IEnumerable<IMessageSegmentsContainer> Values => _containers.Values;

	public IMessageSegmentsContainer this[string key] => _containers[key];

	public void Initialize(Dictionary<string, string> keyMessages)
	{
		ArgumentNullException.ThrowIfNull(keyMessages);

		foreach (var (key, template) in keyMessages)
		{
			var container = new MessageSegmentsContainer(ParseTemplate(template));

			_containers.Add(key, container);
		}
	}

	public bool ContainsKey(string key) => _containers.ContainsKey(key);

	public bool TryGetValue(string key, out IMessageSegmentsContainer value) =>
		_containers.TryGetValue(key, out value!);

	public IEnumerator<KeyValuePair<string, IMessageSegmentsContainer>> GetEnumerator() =>
		_containers.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private static IReadOnlyList<MessageSegment> ParseTemplate(string template)
	{
		ArgumentNullException.ThrowIfNull(template);

		List<MessageSegment> segments = [];

		var textStart = 0;

		for (var i = 0; i < template.Length; i++)
		{
			if (template[i] != '{') continue;

			if (i > textStart) segments.Add(MessageSegment.Text(template[textStart..i]));

			var placeholderStart = i + 1;
			var separatorIndex = -1;
			var endIndex = -1;

			for (var j = placeholderStart; j < template.Length; j++)
			{
				switch (template[j])
				{
					case '|' when separatorIndex < 0:
						separatorIndex = j;
						break;

					case '}':
						endIndex = j;
						break;
				}

				if (endIndex >= 0)
					break;
			}

			if (endIndex < 0)
				throw new FormatException($"Unclosed placeholder in template '{template}'.");

			var placeholderEnd = separatorIndex >= 0 ? separatorIndex : endIndex;

			if (placeholderEnd == placeholderStart)
				throw new FormatException($"Empty placeholder in template '{template}'.");

			var placeholder = template[placeholderStart..placeholderEnd];

			segments.Add(MessageSegment.Placeholder(placeholder));

			i = endIndex;
			textStart = endIndex + 1;
		}

		if (textStart < template.Length)
			segments.Add(MessageSegment.Text(template[textStart..]));

		return segments;
	}
}
