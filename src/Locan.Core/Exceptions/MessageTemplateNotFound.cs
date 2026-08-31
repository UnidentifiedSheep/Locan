namespace Locan.Core.Exceptions;

public sealed class MessageTemplateNotFound : Exception
{
	public MessageTemplateNotFound(string templateKey)
		: base($"Unable to find message template with key '{templateKey}'")
	{
		Data.Add("templateKey", templateKey);
	}
}
