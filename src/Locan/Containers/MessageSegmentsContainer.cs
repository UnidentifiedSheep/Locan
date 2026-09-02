using System.Collections;
using Locan.Core.Enums;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Segments;

namespace Locan.Containers;

internal sealed class MessageSegmentsContainer : IMessageSegmentsContainer
{
	private readonly MessageSegment[] _segments;

	public int Count => _segments.Length;

	public MessageSegment this[int index] => _segments[index];

	public int TextSegmentsTotalLength { get; }

	public MessageSegmentsContainer(IEnumerable<MessageSegment> segments)
	{
		ArgumentNullException.ThrowIfNull(segments);

		_segments = segments.ToArray();

		var textLength = 0;

		for (var i = 0; i < _segments.Length; i++)
		{
			if (_segments[i].Kind == MessageSegmentType.Text)
				textLength += _segments[i].Value.Length;
		}

		TextSegmentsTotalLength = textLength;
	}

	public IEnumerator<MessageSegment> GetEnumerator() => _segments.AsEnumerable().GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
