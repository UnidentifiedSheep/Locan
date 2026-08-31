namespace Locan.Core.Interfaces;

public interface ILocalizableMessage
{
	string MessageKey { get; }
	IReadOnlyDictionary<string, object?> Values { get; }
}
