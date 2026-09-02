using System.Collections;
using System.Collections.Frozen;
using System.Globalization;
using Locan.Core.Enums;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Parsing;

namespace Locan.Containers;

public sealed class SegmentedLocalizerContainer : ILocalizerContainer
{
	private FrozenDictionary<string, IMessageSegmentsContainer>? _containers;

	public SegmentedLocalizerContainer(CultureInfo locale)
	{
		ArgumentNullException.ThrowIfNull(locale);

		Locale = locale;
	}

	public CultureInfo Locale { get; }

	public int Count => Containers.Count;

	public IEnumerable<string> Keys => Containers.Keys;

	public IEnumerable<IMessageSegmentsContainer> Values => Containers.Values;

	public IMessageSegmentsContainer this[string key] => Containers[key];

	public void Initialize(IReadOnlyDictionary<string, string> keyMessages)
	{
		ArgumentNullException.ThrowIfNull(keyMessages);

		if (Volatile.Read(ref _containers) is not null)
			throw new InvalidOperationException("Localizer container is already initialized.");

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

		var containers = parsed.ToFrozenDictionary(StringComparer.Ordinal);

		if (Interlocked.CompareExchange(ref _containers, containers, null) is not null)
			throw new InvalidOperationException("Localizer container is already initialized.");
	}

	public bool ContainsKey(string key) => Containers.ContainsKey(key);

	public bool TryGetValue(string key, out IMessageSegmentsContainer value) =>
		Containers.TryGetValue(key, out value!);

	public IEnumerator<KeyValuePair<string, IMessageSegmentsContainer>> GetEnumerator() =>
		Containers.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	private FrozenDictionary<string, IMessageSegmentsContainer> Containers =>
		Volatile.Read(ref _containers)
		?? throw new InvalidOperationException("Localizer container is not initialized.");
}
