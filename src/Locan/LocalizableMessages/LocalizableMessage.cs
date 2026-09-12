using Locan.Core.Interfaces;
using Locan.Core.Models;

namespace Locan.LocalizableMessages;

public class LocalizableMessage : ILocalizableMessage
{
	public string MessageKey { get; }
	protected readonly Dictionary<string, LocalizableMessageValue> ValuesDict = new();
	public IReadOnlyDictionary<string, LocalizableMessageValue> Values => ValuesDict;

	public LocalizableMessage(string messageKey)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
		MessageKey = messageKey;
	}

	public virtual ILocalizableMessage WithValue(
		string key,
		object? value,
		string? format = null)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ValuesDict[key] = new LocalizableMessageValue(value, format);
		return this;
	}
}
