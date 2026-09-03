using System.Collections;
using System.Collections.Frozen;
using System.Globalization;
using Locan.Core.Enums;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Parsing;

namespace Locan.Containers;

public sealed class SegmentedLocalizerContainer : ILocalizerContainer
{
	private readonly FrozenDictionary<string, IMessageSegmentsContainer> _containers;

	public CultureInfo Culture { get; }
	public int Count => _containers.Count;
	public IEnumerable<string> Keys => _containers.Keys;
	public IEnumerable<IMessageSegmentsContainer> Values => _containers.Values;
	public IMessageSegmentsContainer this[string key] => _containers[key];

	public SegmentedLocalizerContainer(
		CultureInfo culture,
		IReadOnlyDictionary<string, string> keyMessages)
	{
		ArgumentNullException.ThrowIfNull(culture);
		ArgumentNullException.ThrowIfNull(keyMessages);

		Culture = culture;
		var parsed = new Dictionary<string, IMessageSegmentsContainer>(
			keyMessages.Count,
			StringComparer.Ordinal);

		foreach (var (key, template) in keyMessages)
		{
			if (string.IsNullOrWhiteSpace(key))
				throw new ArgumentException("Message key cannot be empty.", nameof(keyMessages));

			if (template is null)
				throw new ArgumentException($"Template for key '{key}' cannot be null.", nameof(keyMessages));

			var segments = MessageTemplateParser.Parse(
				template,
				MessageSegmentType.Text,
				MessageSegmentType.Placeholder);

			parsed.Add(key, new MessageSegmentsContainer(segments));
		}

		_containers = parsed.ToFrozenDictionary(StringComparer.Ordinal);
	}

	public bool ContainsKey(string key) => _containers.ContainsKey(key);

	public bool TryGetValue(string key, out IMessageSegmentsContainer value) =>
		_containers.TryGetValue(key, out value!);

	public IEnumerator<KeyValuePair<string, IMessageSegmentsContainer>> GetEnumerator() =>
		_containers.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
