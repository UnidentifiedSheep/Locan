using Locan.Core.Compatibility;

namespace Locan.Core.Exceptions;

public sealed class MessageTemplateNotFoundException : Exception
{
	public MessageTemplateNotFoundException(string templateKey)
		: base($"Unable to find message template with key '{templateKey}'.")
	{
		ArgumentGuard.NotNullOrWhiteSpace(templateKey, nameof(templateKey));
		Data.Add(nameof(templateKey), templateKey);
	}
}
