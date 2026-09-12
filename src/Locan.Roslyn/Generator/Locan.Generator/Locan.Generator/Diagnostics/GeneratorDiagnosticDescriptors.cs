using Locan.Compiler;
using Microsoft.CodeAnalysis;

namespace Locan.Generator.Diagnostics;

internal static class GeneratorDiagnosticDescriptors
{
	public static readonly DiagnosticDescriptor InvalidModuleName = new(
		"LOCAN001",
		Resource(nameof(Resources.LOCAN001Title)),
		Resource(nameof(Resources.LOCAN001MessageFormat)),
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor InvalidLocalizationFile = new(
		"LOCAN002",
		Resource(nameof(Resources.LOCAN002Title)),
		Resource(nameof(Resources.LOCAN002MessageFormat)),
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor InvalidMessageKey = new(
		"LOCAN003",
		Resource(nameof(Resources.LOCAN003Title)),
		Resource(nameof(Resources.LOCAN003MessageFormat)),
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor ConflictingClassName = new(
		"LOCAN004",
		Resource(nameof(Resources.LOCAN004Title)),
		Resource(nameof(Resources.LOCAN004MessageFormat)),
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	public static readonly DiagnosticDescriptor ConflictingTemplate = new(
		"LOCAN005",
		Resource(nameof(Resources.LOCAN005Title)),
		Resource(nameof(Resources.LOCAN005MessageFormat)),
		"Locan",
		DiagnosticSeverity.Error,
		isEnabledByDefault: true);

	private static LocalizableString Resource(string name) => new LocalizableResourceString(
		name,
		Resources.ResourceManager,
		typeof(Resources));
}
