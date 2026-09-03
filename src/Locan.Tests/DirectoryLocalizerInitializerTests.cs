using System.Globalization;
using Locan.Containers;
using Locan.Core.Exceptions;
using Locan.Initialization;
using Locan.Tests.TestInfrastructure;

namespace Locan.Tests;

public sealed class DirectoryLocalizerInitializerTests
{
	[Fact]
	public async Task InitializeAsync_LoadsLocalizationFile()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"article.en.json",
			"""
			{
			  "culture": "en",
			  "messages": {
			    "article.not.found": "Article not found."
			  }
			}
			""");
		var (initializer, provider) = CreateInitializer();

		await initializer.InitializeAsync(directory.Path);

		var container = provider.GetRequired(CultureInfo.GetCultureInfo("en"));
		Assert.True(container.ContainsKey("article.not.found"));
	}

	[Fact]
	public async Task InitializeAsync_MergesRecursiveFilesWithSameCulture()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"Articles/article.ru-ru.json",
			CreateFile("ru-ru", "article.not.found", "Статья не найдена."));
		await directory.WriteFileAsync(
			"Products/product.ru-ru.json",
			CreateFile("ru-RU", "product.not.found", "Товар не найден."));
		var (initializer, provider) = CreateInitializer();

		await initializer.InitializeAsync(directory.Path);

		var container = provider.GetRequired(CultureInfo.GetCultureInfo("ru-RU"));
		Assert.Equal("ru-RU", container.Culture.Name);
		Assert.Equal(2, container.Count);
		Assert.True(container.ContainsKey("article.not.found"));
		Assert.True(container.ContainsKey("product.not.found"));
	}

	[Fact]
	public async Task InitializeAsync_AllowsSameKeyInDifferentCultures()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"article.en.json",
			CreateFile("en", "article.not.found", "Article not found."));
		await directory.WriteFileAsync(
			"article.de.json",
			CreateFile("de", "article.not.found", "Artikel wurde nicht gefunden."));
		var (initializer, provider) = CreateInitializer();

		await initializer.InitializeAsync(directory.Path);

		Assert.NotNull(provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.NotNull(provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public async Task InitializeAsync_DuplicateKeyPreservesPreviousSnapshot()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"first.ru.json",
			CreateFile("ru", "shared.key", "First"));
		await directory.WriteFileAsync(
			"second.ru.json",
			CreateFile("ru", "shared.key", "Second"));
		var previous = TestFactory.CreateContainer("en");
		var (initializer, provider) = CreateInitializer(previous);

		await Assert.ThrowsAsync<ArgumentException>(
			() => initializer.InitializeAsync(directory.Path));

		Assert.Same(previous, provider.Find(CultureInfo.GetCultureInfo("en")));
		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("ru")));
	}

	[Fact]
	public async Task InitializeAsync_InvalidJsonPreservesPreviousSnapshot()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync("invalid.json", "{ invalid");
		var previous = TestFactory.CreateContainer("en");
		var (initializer, provider) = CreateInitializer(previous);

		await Assert.ThrowsAsync<InvalidDataException>(
			() => initializer.InitializeAsync(directory.Path));

		Assert.Same(previous, provider.Find(CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public async Task InitializeAsync_InvalidTemplatePreservesPreviousSnapshot()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"invalid.en.json",
			CreateFile("en", "invalid", "{Value"));
		var previous = TestFactory.CreateContainer("de");
		var (initializer, provider) = CreateInitializer(previous);

		await Assert.ThrowsAsync<MessageTemplateParseException>(
			() => initializer.InitializeAsync(directory.Path));

		Assert.Same(previous, provider.Find(CultureInfo.GetCultureInfo("de")));
	}

	[Fact]
	public async Task InitializeAsync_RejectsInvalidCulture()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"invalid-culture.json",
			CreateFile("invalid_culture!", "key", "value"));
		var (initializer, _) = CreateInitializer();

		await Assert.ThrowsAsync<InvalidDataException>(
			() => initializer.InitializeAsync(directory.Path));
	}

	[Fact]
	public async Task InitializeAsync_RejectsEmptyCulture()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"empty-culture.json",
			CreateFile(string.Empty, "key", "value"));
		var (initializer, _) = CreateInitializer();

		await Assert.ThrowsAsync<InvalidDataException>(
			() => initializer.InitializeAsync(directory.Path));
	}

	[Fact]
	public async Task InitializeAsync_DirectoryWithoutJsonFilesPublishesEmptySnapshot()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync("ignored.txt", "text");
		var previous = TestFactory.CreateContainer("en");
		var (initializer, provider) = CreateInitializer(previous);

		await initializer.InitializeAsync(directory.Path);

		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public async Task InitializeAsync_DoesNotSearchSubdirectoriesWhenRecursiveIsFalse()
	{
		using var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"Nested/article.en.json",
			CreateFile("en", "article.not.found", "Article not found."));
		var (initializer, provider) = CreateInitializer();

		await initializer.InitializeAsync(directory.Path, recursive: false);

		Assert.Null(provider.Find(CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public async Task InitializeAsync_CanceledTokenPreservesPreviousSnapshot()
	{
		using var directory = new TemporaryDirectory();
		var previous = TestFactory.CreateContainer("en");
		var (initializer, provider) = CreateInitializer(previous);

		await Assert.ThrowsAsync<OperationCanceledException>(
			() => initializer.InitializeAsync(
				directory.Path,
				cancellationToken: new CancellationToken(canceled: true)));

		Assert.Same(previous, provider.Find(CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public async Task InitializeAsync_RejectsMissingDirectory()
	{
		var (initializer, _) = CreateInitializer();
		var path = System.IO.Path.Combine(
			System.IO.Path.GetTempPath(),
			$"Locan.Tests.Missing.{Guid.NewGuid():N}");

		await Assert.ThrowsAsync<DirectoryNotFoundException>(
			() => initializer.InitializeAsync(path));
	}

	private static (DirectoryLocalizerInitializer Initializer, LocalizerContainerProvider Provider)
		CreateInitializer(params SegmentedLocalizerContainer[] containers)
	{
		var provider = TestFactory.CreateProvider(containers);
		return (new DirectoryLocalizerInitializer(provider), provider);
	}

	private static string CreateFile(
		string culture,
		string key,
		string value) => $$"""
		{
		  "culture": "{{culture}}",
		  "messages": {
		    "{{key}}": "{{value}}"
		  }
		}
		""";
}
