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
	IReadOnlyDictionary<string, string?> Values { get; }

	/// <summary>
	/// Allows to set value for the key.
	/// </summary>
	/// <param name="key">Key</param>
	/// <param name="value">Value</param>
	/// <returns></returns>
	ILocalizableMessage WithValue(string key, string? value);
}
