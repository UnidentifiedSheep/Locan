using Locan.Core.Models;

namespace Locan.Core.Interfaces;

public interface ILocalizableMessage
{
	/// <summary>
	/// Key of template/message.
	/// </summary>
	string MessageKey { get; }

	/// <summary>
	/// Entered values
	/// </summary>
	IReadOnlyDictionary<string, LocalizableMessageValue> Values { get; }

	/// <summary>
	/// Allows to set value for the key.
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <param name="format">Optional format string</param>
	/// <returns></returns>
	ILocalizableMessage WithValue(string key, object? value, string? format = null);
}
