using System.Collections.Generic;

namespace Locan.Generator.Models;

internal sealed class MessageDefinition
{
	public MessageDefinition(
		string key,
		string className,
		IReadOnlyDictionary<string, PlaceholderInfo> placeholders)
	{
		Key = key;
		ClassName = className;
		Placeholders = placeholders;
	}

	public string Key { get; }

	public string ClassName { get; }

	public IReadOnlyDictionary<string, PlaceholderInfo> Placeholders { get; }
}
