using System;
using System.Linq;
using Locan.Generator.Models;
using Locan.Generator.Sources;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Locan.Generator.Extensions;

internal static class LocalizationModuleProviderExtensions
{
	public static IncrementalValuesProvider<ModuleOptions> ForLocalizationModules(
		this SyntaxValueProvider provider)
	{
		return provider
			.CreateSyntaxProvider(
				static (node, _) => IsCandidate(node),
				static (context, _) => GetModule(context))
			.Where(static module => module is not null)
			.Select(static (module, _) => module!);
	}

	private static bool IsCandidate(SyntaxNode node)
	{
		return node is CompilationUnitSyntax compilationUnit &&
			compilationUnit.AttributeLists.Any(static list =>
				list.Target?.Identifier.IsKind(SyntaxKind.AssemblyKeyword) == true &&
				list.Attributes.Any(static attribute => IsAttributeName(attribute.Name)));
	}

	private static ModuleOptions? GetModule(GeneratorSyntaxContext context)
	{
		var compilationUnit = (CompilationUnitSyntax)context.Node;
		var attribute = compilationUnit.AttributeLists
			.Where(static list =>
				list.Target?.Identifier.IsKind(SyntaxKind.AssemblyKeyword) == true)
			.SelectMany(static list => list.Attributes)
			.FirstOrDefault(static attribute => IsAttributeName(attribute.Name));

		if (attribute?.ArgumentList is null || attribute.ArgumentList.Arguments.Count == 0)
			return null;

		var nameConstant = context.SemanticModel.GetConstantValue(
			attribute.ArgumentList.Arguments[0].Expression);

		if (!nameConstant.HasValue || nameConstant.Value is not string name)
			return null;

		var isInternalArgument = attribute.ArgumentList.Arguments
			.FirstOrDefault(static argument =>
				argument.NameEquals?.Name.Identifier.ValueText == "IsInternal");
		var isInternal = false;

		if (isInternalArgument is not null)
		{
			var constant = context.SemanticModel.GetConstantValue(
				isInternalArgument.Expression);
			isInternal = constant.HasValue && constant.Value is true;
		}

		return new ModuleOptions(name, isInternal);
	}

	private static bool IsAttributeName(NameSyntax name)
	{
		var identifier = name switch
		{
			IdentifierNameSyntax identifierName => identifierName.Identifier.ValueText,
			QualifiedNameSyntax qualifiedName => qualifiedName.Right.Identifier.ValueText,
			AliasQualifiedNameSyntax aliasQualifiedName => aliasQualifiedName.Name.Identifier.ValueText,
			_ => string.Empty
		};

		return string.Equals(
				name.ToString(),
				LocalizationModuleAttributeSource.Name,
				StringComparison.Ordinal) ||
			identifier is "LocalizationModule" or "LocalizationModuleAttribute";
	}
}
