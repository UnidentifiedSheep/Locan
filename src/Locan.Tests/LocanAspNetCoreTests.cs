using System.Globalization;
using Locan.AspNetCore;
using Locan.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Locan.Tests;

public sealed class LocanAspNetCoreTests
{
	[Fact]
	public void AddLocanAspNetCore_ConfiguresRequestLocalization()
	{
		var services = new ServiceCollection();
		services.AddLocanAspNetCore(options =>
		{
			options.DefaultCulture = "en-US";
			options.SupportedCultures = ["de-DE"];
		});

		using var provider = services.BuildServiceProvider();
		var options = provider
			.GetRequiredService<IOptions<RequestLocalizationOptions>>()
			.Value;

		Assert.Equal("en-US", options.DefaultRequestCulture.Culture.Name);
		Assert.Equal(
			["en-US", "de-DE"],
			options.SupportedCultures!.Select(static culture => culture.Name));
		Assert.Equal(
			["en-US", "de-DE"],
			options.SupportedUICultures!.Select(static culture => culture.Name));
	}

	[Fact]
	public void AddLocanAspNetCore_BindsHostingAndRequestOptions()
	{
		var values = new Dictionary<string, string?>
		{
			[$"{LocanOptions.SectionName}:DirectoryPath"] = "/localization",
			[$"{LocanOptions.SectionName}:SearchPattern"] = "*.locan.json",
			[$"{LocanOptions.SectionName}:Recursive"] = "false",
			[$"{LocanOptions.SectionName}:{LocanRequestLocalizationOptions.SectionName}:DefaultCulture"] = "ru-RU",
			[$"{LocanOptions.SectionName}:{LocanRequestLocalizationOptions.SectionName}:SupportedCultures:0"] = "ru-RU",
			[$"{LocanOptions.SectionName}:{LocanRequestLocalizationOptions.SectionName}:SupportedCultures:1"] = "en-US"
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(values)
			.Build();
		var services = new ServiceCollection();

		services.AddLocanAspNetCore(configuration.GetSection(LocanOptions.SectionName));

		using var provider = services.BuildServiceProvider();
		var hosting = provider.GetRequiredService<IOptions<LocanOptions>>().Value;
		var request = provider.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
		Assert.Equal("/localization", hosting.DirectoryPath);
		Assert.Equal("*.locan.json", hosting.SearchPattern);
		Assert.False(hosting.Recursive);
		Assert.Equal("ru-RU", request.DefaultRequestCulture.Culture.Name);
		Assert.Equal(
			["ru-RU", "en-US"],
			request.SupportedUICultures!.Select(static culture => culture.Name));
	}

	[Fact]
	public async Task UseLocanRequestLocalization_UsesStandardMiddleware()
	{
		var services = new ServiceCollection();
		services.AddLogging();
		services.AddLocanAspNetCore(options =>
		{
			options.DefaultCulture = "en-US";
			options.SupportedCultures = ["en-US", "de-DE"];
		});
		await using var provider = services.BuildServiceProvider();
		var application = new ApplicationBuilder(provider);
		string? culture = null;
		application.UseLocanRequestLocalization();
		application.Run(_ =>
		{
			culture = CultureInfo.CurrentUICulture.Name;
			return Task.CompletedTask;
		});
		var pipeline = application.Build();
		var context = new DefaultHttpContext();
		context.Request.Headers.AcceptLanguage = "de-DE";

		await pipeline(context);

		Assert.Equal("de-DE", culture);
	}

	[Fact]
	public void AddLocanAspNetCore_RejectsInvalidCulture()
	{
		var services = new ServiceCollection();
		services.AddLocanAspNetCore(options => options.DefaultCulture = "invalid_culture!");
		using var provider = services.BuildServiceProvider();

		Assert.Throws<OptionsValidationException>(
			() => provider
				.GetRequiredService<IOptions<RequestLocalizationOptions>>()
				.Value);
	}

	[Fact]
	public void AddLocanAspNetCore_RejectsNullSupportedCultures()
	{
		var services = new ServiceCollection();
		services.AddLocanAspNetCore(options => options.SupportedCultures = null!);
		using var provider = services.BuildServiceProvider();

		Assert.Throws<OptionsValidationException>(
			() => provider
				.GetRequiredService<IOptions<RequestLocalizationOptions>>()
				.Value);
	}
}
