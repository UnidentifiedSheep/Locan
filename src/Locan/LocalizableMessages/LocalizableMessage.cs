using Locan.Core.Interfaces;

namespace Locan.LocalizableMessages;

public abstract class LocalizableMessage : ILocalizableMessage
{
	public string MessageKey { get; }
	protected readonly Dictionary<string, string?> ValuesDict = new();
	public IReadOnlyDictionary<string, string?> Values => ValuesDict;

	protected LocalizableMessage(string messageKey)
	{
		MessageKey = messageKey;
	}

	public virtual ILocalizableMessage WithValue(string key, string? value)
	{
		ValuesDict[key] = value;
		return this;
	}
}
