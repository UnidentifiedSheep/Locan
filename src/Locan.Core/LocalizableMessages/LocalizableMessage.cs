using Locan.Core.Compatibility;
using Locan.Core.Interfaces;
using Locan.Core.Models;

namespace Locan.Core.LocalizableMessages;

public class LocalizableMessage : ILocalizableMessage
{
	public string MessageKey { get; }
	protected readonly Dictionary<string, LocalizableMessageValue> ValuesDict;
	public IReadOnlyDictionary<string, LocalizableMessageValue> Values => ValuesDict;

	public LocalizableMessage(string messageKey, int capacity = 0)
	{
		ArgumentGuard.NotNullOrWhiteSpace(messageKey, nameof(messageKey));
		MessageKey = messageKey;
		ValuesDict = new Dictionary<string, LocalizableMessageValue>(capacity);
	}

	public virtual ILocalizableMessage WithValue(
		string key,
		object? value,
		string? format = null)
	{
		ArgumentGuard.NotNullOrWhiteSpace(key, nameof(key));
		ValuesDict[key] = new LocalizableMessageValue(value, format);
		return this;
	}
}
