using Locan.Core.Segments;

namespace Locan.Core.Interfaces.Containers;

public interface IMessageSegmentsContainer : IReadOnlyList<MessageSegment>
{
	int TextSegmentsTotalLength { get; }
}
