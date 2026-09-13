using System.Linq;
using Locan.Generator.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Locan.Generator.Extensions;

internal static class LocalizationModuleProviderExtensions
{
	private const string AttributeName =
		"Locan.Core.Attributes.LocalizationModuleAttribute";

	public static IncrementalValuesProvider<ModuleOptions> ForLocalizationModules(
		this SyntaxValueProvider provider)
	{
		return provider
			.ForAttributeWithMetadataName(
				AttributeName,
				static (node, _) => node is CompilationUnitSyntax,
				static (context, _) => GetModule(context))
			.Where(static module => module is not null)
			.Select(static (module, _) => module!);
	}

	private static ModuleOptions? GetModule(GeneratorAttributeSyntaxContext context)
	{
		var attribute = context.Attributes.FirstOrDefault();

		if (attribute is null ||
			attribute.ConstructorArguments.Length == 0 ||
			attribute.ConstructorArguments[0].Value is not string name)
			return null;

		var isInternal = attribute.NamedArguments
			.FirstOrDefault(static argument => argument.Key == "IsInternal")
			.Value.Value is true;

		return new ModuleOptions(name, isInternal);
	}
}
