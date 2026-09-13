using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Locan.Generator.Diagnostics;
using Locan.Generator.Extensions;
using Locan.Generator.Generation;
using Locan.Generator.Models;
using Locan.Generator.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Locan.Generator;

[Generator]
public sealed class LocalizableMessagesGenerator : IIncrementalGenerator
{
	private const string DefaultCultureProperty = "build_property.LocanDefaultCulture";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		var modules = context.SyntaxProvider.ForLocalizationModules();
		var defaultCulture = context.AnalyzerConfigOptionsProvider.Select(
			static (provider, _) =>
				provider.GlobalOptions.TryGetValue(DefaultCultureProperty, out var value) &&
				!string.IsNullOrWhiteSpace(value)
					? value
					: "en");
		var input = modules
			.Combine(context.AdditionalTextsProvider.Collect())
			.Combine(defaultCulture);

		context.RegisterSourceOutput(
			input,
			static (sourceContext, value) => GenerateCode(
				sourceContext,
				value.Left.Left,
				value.Left.Right,
				value.Right));
	}

	private static void GenerateCode(
		SourceProductionContext context,
		ModuleOptions module,
		ImmutableArray<AdditionalText> files,
		string defaultCulture)
	{
		if (!CSharpNames.IsValidNamespace(module.Name))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				GeneratorDiagnosticDescriptors.InvalidModuleName,
				Location.None,
				module.Name));
			return;
		}

		var catalog = new MessageCatalogBuilder();

		foreach (var file in files.OrderBy(static file => file.Path, StringComparer.Ordinal))
		{
			if (!LocalizationResourceParser.TryParse(
					file,
					context.CancellationToken,
					out var resource,
					out var error))
			{
				context.ReportDiagnostic(Diagnostic.Create(
					GeneratorDiagnosticDescriptors.InvalidLocalizationFile,
					Location.None,
					file.Path,
					error));
				continue;
			}

			if (!resource!.IsTemplate && !string.Equals(
					resource.Culture,
					defaultCulture,
					StringComparison.OrdinalIgnoreCase))
				continue;

			foreach (var message in resource.Messages
				         .OrderBy(static pair => pair.Key, StringComparer.Ordinal))
				catalog.Add(message.Key, message.Value);
		}

		foreach (var diagnostic in catalog.Diagnostics)
			context.ReportDiagnostic(diagnostic);

		foreach (var definition in catalog.Definitions.OrderBy(
			static definition => definition.ClassName,
			StringComparer.Ordinal))
			context.AddSource(
				$"{definition.ClassName}.g.cs",
				SourceText.From(
					MessageClassSourceBuilder.Build(module, definition),
					Encoding.UTF8));
	}
}
