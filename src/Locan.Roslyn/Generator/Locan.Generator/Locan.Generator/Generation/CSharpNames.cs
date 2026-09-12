using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Locan.Generator.Generation;

internal static class CSharpNames
{
	private static readonly Regex ClassNameSeparators = new(@"[^\p{L}\p{Nd}]+");

	public static string NormalizeClassName(string key)
	{
		var parts = ClassNameSeparators
			.Split(key)
			.Where(static part => part.Length > 0);
		var name = string.Concat(parts.Select(ToPascalCase));

		if (name.Length == 0)
			return string.Empty;

		return char.IsDigit(name[0]) ? "_" + name : name;
	}

	public static string GetTypeName(string? type)
	{
		var normalizedType = type?.Trim();
		var isNullable = normalizedType?.EndsWith("?", StringComparison.Ordinal) == true;

		if (isNullable)
			normalizedType = normalizedType!.Substring(0, normalizedType.Length - 1);

		var typeName = normalizedType?.ToLowerInvariant() switch
		{
			"bool" or "boolean" => "bool",
			"byte" => "byte",
			"sbyte" => "sbyte",
			"short" or "int16" => "short",
			"ushort" or "uint16" => "ushort",
			"int" or "int32" => "int",
			"uint" or "uint32" => "uint",
			"long" or "int64" => "long",
			"ulong" or "uint64" => "ulong",
			"float" or "single" => "float",
			"double" => "double",
			"decimal" => "decimal",
			"char" => "char",
			"object" => "object",
			"datetime" => "global::System.DateTime",
			_ => "string"
		};

		return isNullable ? typeName + "?" : typeName;
	}

	public static bool IsValidNamespace(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
			return false;

		return value
			.Split('.')
			.All(static part => SyntaxFacts.IsValidIdentifier(part));
	}

	public static string EscapeIdentifier(string value)
	{
		return SyntaxFacts.GetKeywordKind(value) == SyntaxKind.None
			? value
			: "@" + value;
	}

	public static string Literal(string? value)
	{
		return value is null
			? "null"
			: SymbolDisplay.FormatLiteral(value, quote: true);
	}

	private static string ToPascalCase(string value)
	{
		if (value.Length == 0) return value;
		return char.ToUpperInvariant(value[0]) + value.Substring(1);
	}
}
