using System.Collections.Generic;

namespace Locan.Generator.Models;

internal sealed class MessageDefinition
{
	public MessageDefinition(
		string key,
		string className,
		string template,
		IReadOnlyDictionary<string, PlaceholderInfo> placeholders)
	{
		Key = key;
		ClassName = className;
		Template = template;
		Placeholders = placeholders;
	}

	public string Key { get; }

	public string ClassName { get; }

	public string Template { get; }

	public IReadOnlyDictionary<string, PlaceholderInfo> Placeholders { get; }
}
