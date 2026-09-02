namespace Locan.Core.Exceptions;

public sealed class MessageTemplateNotFoundException : Exception
{
	public MessageTemplateNotFoundException(string templateKey)
		: base($"Unable to find message template with key '{templateKey}'.")
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(templateKey);
		Data.Add(nameof(templateKey), templateKey);
	}
}
