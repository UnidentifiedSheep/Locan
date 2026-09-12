using System.Collections.Immutable;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Locan.Core.Interfaces;
using Locan.Generator.Tests.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Locan.Generator.Tests;

public sealed class LocalizableMessagesGeneratorTests
{
	private const string LocalizationJson = """
		{
		  "culture": "en",
		  "messages": {
		    "user.created": "User {Name|String} was created",
		    "invoice.paid": "Invoice {Number|int} was paid at {Date|dateTIME|yyyy-MM-dd} by {Source|UnknownType}",
		    "nullable.values": "{Count|int?} {Date|DateTime?} {Text|String?} {Fallback|UnknownType?}",
		    "with.name": "{NameMessage|String} {nameMessage|String}"
		  }
		}
		""";

	[Fact]
	public void SampleProjectGeneratesMessagesFromLocalizationFiles()
	{
		var notFound = new Sample.ArticleNotFoundMessage();
		var stock = Sample.ArticleStockChangedMessage.Create(
			"SKU-42",
			-3);
		var reservation = Sample.ArticleReservationStatusChangedMessage.Create(
			42L,
			true);
		var price = Sample.ArticlePriceUpdatedMessage.Create(
			"SKU-42",
			12.5m,
			"USD");
		var completed = Sample.ArticleImportCompletedMessage.Create(
			new DateTime(2026, 9, 12, 13, 45, 10, DateTimeKind.Utc),
			1234);

		Assert.Equal("article.not.found", notFound.MessageKey);
		Assert.Equal("SKU-42", stock.Values["Sku"].Value);
		Assert.Equal(-3, stock.Values["Delta"].Value);
		Assert.Equal(42L, reservation.Values["ReservationId"].Value);
		Assert.Equal(true, reservation.Values["IsActive"].Value);
		Assert.Equal(12.5m, price.Values["Price"].Value);
		Assert.Equal("F2", price.Values["Price"].Format);
		Assert.Equal(
			new DateTime(2026, 9, 12, 13, 45, 10, DateTimeKind.Utc),
			completed.Values["CompletedAt"].Value);
		Assert.Equal("yyyy-MM-dd HH:mm:ss", completed.Values["CompletedAt"].Format);
		Assert.Equal(1234, completed.Values["Count"].Value);
		Assert.Equal("N0", completed.Values["Count"].Format);
	}

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
		Assert.Contains(
			"Default culture template: <c>Invoice {Number|int} was paid at {Date|dateTIME|yyyy-MM-dd} by {Source|UnknownType}</c>",
			invoiceSource);
		Assert.Contains(
			"<param name=\"Date\">Type: <c>global::System.DateTime</c>. Format: <c>yyyy-MM-dd</c>.</param>",
			invoiceSource);
		Assert.Contains("message.WithValue(\"Date\", Date, \"yyyy-MM-dd\");", invoiceSource);
		Assert.DoesNotContain("FormatValue", invoiceSource);
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

	[Fact]
	public void GeneratesMessagesOnlyForConfiguredDefaultCulture()
	{
		var driver = CreateDriver(
			"ru",
			new TestAdditionalFile(
				"./messages.en.json",
				CreateResource("en", "english")),
			new TestAdditionalFile(
				"./messages.ru.json",
				CreateResource("ru", "russian")));

		var result = RunGenerator(driver);
		var generatedFiles = result.SyntaxTrees
			.Skip(1)
			.Select(static tree => Path.GetFileName(tree.FilePath))
			.ToArray();

		Assert.Contains("RussianMessage.g.cs", generatedFiles);
		Assert.DoesNotContain("EnglishMessage.g.cs", generatedFiles);
	}

	[Fact]
	public void GeneratesMessagesForExplicitTemplateOutsideDefaultCulture()
	{
		var driver = CreateDriver(
			"en",
			new TestAdditionalFile(
				"./messages.ru.json",
				CreateResource("ru", "shared", isTemplate: true)));

		var result = RunGenerator(driver);

		Assert.Contains(
			result.SyntaxTrees,
			static tree => Path.GetFileName(tree.FilePath) == "SharedMessage.g.cs");
	}

	private static GeneratorDriver CreateDriver(
		string defaultCulture,
		params AdditionalText[] files)
	{
		var options = new TestAnalyzerConfigOptionsProvider(
			new Dictionary<string, string>
			{
				["build_property.LocanDefaultCulture"] = defaultCulture
			});

		return CSharpGeneratorDriver.Create(
			[new LocalizableMessagesGenerator().AsSourceGenerator()],
			files,
			optionsProvider: options);
	}

	private static Compilation RunGenerator(GeneratorDriver driver)
	{
		var source = """
			[assembly: Locan.Generator.Attributes.LocalizationModule("Sample.Messages")]
			""";
		var compilation = CSharpCompilation.Create(
			"GeneratorTest",
			[CSharpSyntaxTree.ParseText(source)],
			GetMetadataReferences(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		driver.RunGeneratorsAndUpdateCompilation(
			compilation,
			out var newCompilation,
			out var diagnostics);
		Assert.Empty(diagnostics);
		return newCompilation;
	}

	private static string CreateResource(
		string culture,
		string key,
		bool isTemplate = false) => $$"""
		{
		  "culture": "{{culture}}",
		  "isTemplate": {{isTemplate.ToString().ToLowerInvariant()}},
		  "messages": {
		    "{{key}}": "Message"
		  }
		}
		""";

	private static MetadataReference[] GetMetadataReferences()
	{
		var frameworkAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(static path => MetadataReference.CreateFromFile(path));

		return
		[
			.. frameworkAssemblies,
			MetadataReference.CreateFromFile(typeof(ILocalizableMessage).Assembly.Location)
		];
	}
}
