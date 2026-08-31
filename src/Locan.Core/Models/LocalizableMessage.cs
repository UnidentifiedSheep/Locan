namespace Locan.Core.Models;

public class LocalizableMessage
{
	private readonly Dictionary<string, object?> _values = [];
	private readonly string _messageKey;

	public IReadOnlyDictionary<string, object?> Values => _values;

	public LocalizableMessage(string messageKey)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(messageKey);
		_messageKey = messageKey.Trim();
	}

	public LocalizableMessage WithValue<TValue>(string key, TValue? value)
	{
		_values[key] = value;
		return this;
	}

	public static implicit operator string(LocalizableMessage messageKey) => messageKey._messageKey;
	public static implicit operator LocalizableMessage(string messageKey) => new(messageKey);
}
