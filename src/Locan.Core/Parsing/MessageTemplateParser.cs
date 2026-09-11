using Locan.Core.Enums;
using Locan.Core.Segments;

namespace Locan.Core.Parsing;

public static class MessageTemplateParser
{
	public static IReadOnlyList<MessageSegment> Parse(
		string template,
		params MessageSegmentType[] allowed)
	{
		if (template is null)
			throw new ArgumentNullException(nameof(template));

		if (allowed is null)
			throw new ArgumentNullException(nameof(allowed));

		var syntax = TemplateSyntaxParser.Parse(template);
		MessageTemplateValidator.Validate(syntax);
		return MessageSegmentProjector.Project(syntax, allowed);
	}
}
