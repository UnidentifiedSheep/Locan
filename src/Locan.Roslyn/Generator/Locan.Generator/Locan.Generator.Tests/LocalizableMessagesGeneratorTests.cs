using System.Collections.Immutable;
using System;
using System.IO;
using System.Linq;
using Locan.Core.Interfaces;
using Locan.Generator.Tests.Utils;
using Locan.LocalizableMessages;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Locan.Generator.Tests;

public sealed class LocalizableMessagesGeneratorTests
{
	private const string LocalizationJson = """
		{
		  "culture": "en",
		  "isTemplate": true,
		  "messages": {
		    "user.created": "User {Name|String} was created",
		    "invoice.paid": "Invoice {Number|int} was paid at {Date|dateTIME|yyyy-MM-dd} by {Source|UnknownType}",
		    "nullable.values": "{Count|int?} {Date|DateTime?} {Text|String?} {Fallback|UnknownType?}",
		    "with.name": "{NameMessage|String} {nameMessage|String}"
		  }
		}
		""";

	[Fact]
	public void GeneratesMessagesForAttributedAssembly()
	{
		var generator = new LocalizableMessagesGenerator();
		GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

		driver = driver.AddAdditionalTexts(
			ImmutableArray.Create<AdditionalText>(
				new TestAdditionalFile("./localization.en.json", LocalizationJson)));

		var source = """
			[assembly: Locan.Generator.Attributes.LocalizationModule("Sample.Messages")]
			""";
		var compilation = CSharpCompilation.Create(
			nameof(LocalizableMessagesGeneratorTests),
			[CSharpSyntaxTree.ParseText(source)],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out var newCompilation,
			out var diagnostics);

		Assert.Empty(diagnostics);
		Assert.Equivalent(
			new[]
			{
				"LocalizationModuleAttribute.g.cs",
				"UserCreatedMessage.g.cs",
				"InvoicePaidMessage.g.cs",
				"NullableValuesMessage.g.cs",
				"WithNameMessage.g.cs"
			},
			newCompilation.SyntaxTrees
				.Skip(1)
				.Select(static tree => Path.GetFileName(tree.FilePath)));
		Assert.Empty(newCompilation.GetDiagnostics().Where(static diagnostic =>
			diagnostic.Severity == DiagnosticSeverity.Error));

		var invoiceSource = newCompilation.SyntaxTrees
			.Single(static tree => Path.GetFileName(tree.FilePath) == "InvoicePaidMessage.g.cs")
			.ToString();

		Assert.Contains(
			"Create(int Number, global::System.DateTime Date, string Source)",
			invoiceSource);
		Assert.Contains("public InvoicePaidMessage WithNumber(int value)", invoiceSource);
		Assert.Contains("return this;", invoiceSource);

		var nullableSource = newCompilation.SyntaxTrees
			.Single(static tree => Path.GetFileName(tree.FilePath) == "NullableValuesMessage.g.cs")
			.ToString();

		Assert.Contains(
			"Create(int? Count, global::System.DateTime? Date, string? Text, string? Fallback)",
			nullableSource);

		var conflictingNameSource = newCompilation.SyntaxTrees
			.Single(static tree => Path.GetFileName(tree.FilePath) == "WithNameMessage.g.cs")
			.ToString();

		Assert.Contains("WithNameMessagePlaceholder(string value)", conflictingNameSource);
		Assert.Contains("WithNameMessagePlaceholder2(string value)", conflictingNameSource);
	}

	private static MetadataReference[] GetMetadataReferences()
	{
		var frameworkAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(static path => MetadataReference.CreateFromFile(path));

		return frameworkAssemblies
			.Append(MetadataReference.CreateFromFile(typeof(ILocalizableMessage).Assembly.Location))
			.Append(MetadataReference.CreateFromFile(typeof(LocalizableMessage).Assembly.Location))
			.ToArray();
	}
}
