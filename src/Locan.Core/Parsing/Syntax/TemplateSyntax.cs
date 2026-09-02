namespace Locan.Core.Parsing.Syntax;

internal abstract record TemplateSyntax(int Position);

internal sealed record TextSyntax(
	int Position,
	string Value) : TemplateSyntax(Position);

internal sealed record PlaceholderSyntax(
	int Position,
	string Key,
	string? ValueType,
	string? Format) : TemplateSyntax(Position);
