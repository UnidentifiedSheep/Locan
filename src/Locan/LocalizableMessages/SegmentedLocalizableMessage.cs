using Locan.Core.Interfaces;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Segments;

namespace Locan.LocalizableMessages;

public class SegmentedLocalizableMessage : LocalizableMessage
{
	public IMessageSegmentsContainer Segments { get; }

	public SegmentedLocalizableMessage(
		string messageKey,
		IEnumerable<MessageSegment> segments) : base(messageKey)
	{
		Segments = new MessageSegmentsContainer(segments);
	}

	public SegmentedLocalizableMessage(
		string messageKey,
		IMessageSegmentsContainer segments) : base(messageKey)
	{
		Segments = segments;
	}
}
