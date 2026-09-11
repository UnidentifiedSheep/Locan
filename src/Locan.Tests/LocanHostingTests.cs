using System.Globalization;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Initialization;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.Interfaces.Rendering;
using Locan.Core.Models;
using Locan.Hosting;
using Locan.Tests.TestInfrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Locan.Tests;

public sealed class LocanHostingTests
{
	[Fact]
	public void AddLocan_RegistersRuntimeServices()
	{
		var services = new ServiceCollection();

		services.AddLocan();

		using var provider = services.BuildServiceProvider();
		Assert.NotNull(provider.GetRequiredService<ILocalizerContainerProvider>());
		Assert.NotNull(provider.GetRequiredService<ILocalizerContainerRegistry>());
		Assert.NotNull(provider.GetRequiredService<ILocalizerInitializer>());
		Assert.NotNull(provider.GetRequiredService<IMessageTemplateRenderer>());
		Assert.NotNull(provider.GetRequiredService<ILocalizer>());
		Assert.NotNull(provider.GetRequiredService<IContextualLocalizer>());
		Assert.Single(provider.GetServices<IHostedService>());
	}

	[Fact]
	public void AddLocan_UsesSameInstanceForProviderAndRegistry()
	{
		var services = new ServiceCollection();
		services.AddLocan();

		using var provider = services.BuildServiceProvider();

		Assert.Same(
			provider.GetRequiredService<ILocalizerContainerProvider>(),
			provider.GetRequiredService<ILocalizerContainerRegistry>());
	}

	[Fact]
	public void AddLocan_BindsOptionsFromConfiguration()
	{
		var values = new Dictionary<string, string?>
		{
			["Locan:DirectoryPath"] = "/localization",
			["Locan:SearchPattern"] = "*.locan.json",
			["Locan:Recursive"] = "false"
		};
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(values)
			.Build();
		var services = new ServiceCollection();

		services.AddLocan(configuration.GetSection(LocanOptions.SectionName));

		using var provider = services.BuildServiceProvider();
		var options = provider.GetRequiredService<IOptions<LocanOptions>>().Value;
		Assert.Equal("/localization", options.DirectoryPath);
		Assert.Equal("*.locan.json", options.SearchPattern);
		Assert.False(options.Recursive);
	}

	[Fact]
	public async Task HostedService_LoadsResourcesOnStart()
	{
		var directory = new TemporaryDirectory();
		await directory.WriteFileAsync(
			"article.en.locan.json",
			"""
			{
			  "culture": "en",
			  "messages": {
			    "article.not.found": "Article not found."
			  }
			}
			""");
		var services = new ServiceCollection();
		services.AddLocan(options =>
		{
			options.DirectoryPath = directory.Path;
			options.SearchPattern = "*.locan.json";
		});
		await using var provider = services.BuildServiceProvider();
		var hostedService = Assert.Single(provider.GetServices<IHostedService>());

		await hostedService.StartAsync(CancellationToken.None);

		var containers = provider.GetRequiredService<ILocalizerContainerProvider>();
		Assert.NotNull(containers.Find(CultureInfo.GetCultureInfo("en")));
	}

	[Fact]
	public async Task HostedService_RejectsInvalidOptions()
	{
		var services = new ServiceCollection();
		services.AddLocan(options => options.DirectoryPath = string.Empty);
		await using var provider = services.BuildServiceProvider();
		var hostedService = Assert.Single(provider.GetServices<IHostedService>());

		await Assert.ThrowsAsync<OptionsValidationException>(
			() => hostedService.StartAsync(CancellationToken.None));
	}

	[Fact]
	public void AddLocan_CalledTwiceRegistersSingleHostedService()
	{
		var services = new ServiceCollection();

		services.AddLocan();
		services.AddLocan();

		using var provider = services.BuildServiceProvider();
		Assert.Single(provider.GetServices<IHostedService>());
	}
}
