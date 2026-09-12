using Microsoft.CodeAnalysis;

namespace Locan.Generator.Diagnostics;

internal static class GeneratorDiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor InvalidModuleName = new(
		"LOCAN001",
		"Invalid localization module name",
		"'{0}' is not a valid C# namespace",
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor InvalidLocalizationFile = new(
		"LOCAN002",
		"Invalid localization file",
		"Localization file '{0}' is invalid: {1}",
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor InvalidMessageKey = new(
		"LOCAN003",
		"Invalid localization message key",
		"Message key '{0}' cannot be converted to a valid C# class name",
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor ConflictingClassName = new(
		"LOCAN004",
		"Conflicting localization message class name",
		"Message keys '{0}' and '{1}' both produce class name '{2}'",
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor ConflictingTemplate = new(
		"LOCAN005",
		"Conflicting localization templates",
		"Message key '{0}' has incompatible placeholders in template files",
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);
}
