using Locan.Core.Interfaces;

namespace Locan.LocalizableMessages;

public class LocalizableMessage : ILocalizableMessage
{
	public string MessageKey { get; }
	protected readonly Dictionary<string, string?> ValuesDict = new();
	public IReadOnlyDictionary<string, string?> Values => ValuesDict;

	public LocalizableMessage(string messageKey)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
		MessageKey = messageKey;
	}

	public virtual ILocalizableMessage WithValue(string key, string? value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(key);
		ValuesDict[key] = value;
		return this;
	}
}
