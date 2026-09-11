using Locan.Containers;
using Locan.Core.Interfaces.Containers;
using Locan.Core.Interfaces.Initialization;
using Locan.Core.Interfaces.Localizers;
using Locan.Core.Interfaces.Rendering;
using Locan.Core.Models;
using Locan.Initialization;
using Locan.TemplateRenderers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Locan.Hosting;

public static class LocanServiceCollectionExtensions
{
	public static IServiceCollection AddLocan(
		this IServiceCollection services,
		Action<LocanOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		var options = services
			.AddOptions<LocanOptions>()
			.Validate(
				static value => !string.IsNullOrWhiteSpace(value.DirectoryPath),
				$"{nameof(LocanOptions.DirectoryPath)} cannot be empty.")
			.Validate(
				static value => !string.IsNullOrWhiteSpace(value.SearchPattern),
				$"{nameof(LocanOptions.SearchPattern)} cannot be empty.");

		if (configure is not null)
			options.Configure(configure);

		AddServices(services);
		return services;
	}

	public static IServiceCollection AddLocan(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services
			.AddOptions<LocanOptions>()
			.Bind(configuration)
			.Validate(
				static value => !string.IsNullOrWhiteSpace(value.DirectoryPath),
				$"{nameof(LocanOptions.DirectoryPath)} cannot be empty.")
			.Validate(
				static value => !string.IsNullOrWhiteSpace(value.SearchPattern),
				$"{nameof(LocanOptions.SearchPattern)} cannot be empty.");

		AddServices(services);
		return services;
	}

	private static void AddServices(IServiceCollection services)
	{
		services.TryAddSingleton<LocalizerContainerProvider>();
		services.TryAddSingleton<ILocalizerContainerProvider>(
			static provider => provider.GetRequiredService<LocalizerContainerProvider>());
		services.TryAddSingleton<ILocalizerContainerRegistry>(
			static provider => provider.GetRequiredService<LocalizerContainerProvider>());
		services.TryAddSingleton<ILocalizerInitializer, DirectoryLocalizerInitializer>();
		services.TryAddSingleton<IMessageTemplateRenderer, SegmentedMessageTemplateRenderer>();
		services.TryAddSingleton<ILocalizer, Localizer>();
		services.TryAddSingleton<IContextualLocalizer, ContextualLocalizer>();
		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<IHostedService, LocalizerInitializationHostedService>());
	}
}
