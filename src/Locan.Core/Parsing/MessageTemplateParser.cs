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

		var syntax = TemplateSyntaxParser.Parse(template);
		MessageTemplateValidator.Validate(syntax);
		return MessageSegmentProjector.Project(syntax, allowed);
	}
}
