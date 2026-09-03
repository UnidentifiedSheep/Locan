using System.Globalization;
using Locan.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace Locan.AspNetCore;

public static class LocanAspNetCoreServiceCollectionExtensions
{
	public static IServiceCollection AddLocanAspNetCore(
		this IServiceCollection services,
		Action<LocanRequestLocalizationOptions>? configure = null)
	{
		ArgumentNullException.ThrowIfNull(services);

		services.AddLocan();
		var options = AddRequestLocalizationOptions(services);

		if (configure is not null)
			options.Configure(configure);

		return services;
	}

	public static IServiceCollection AddLocanAspNetCore(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		ArgumentNullException.ThrowIfNull(services);
		ArgumentNullException.ThrowIfNull(configuration);

		services.AddLocan(configuration);
		AddRequestLocalizationOptions(services)
			.Bind(configuration.GetSection(LocanRequestLocalizationOptions.SectionName));

		return services;
	}

	private static OptionsBuilder<LocanRequestLocalizationOptions> AddRequestLocalizationOptions(
		IServiceCollection services)
	{
		services.AddLocalization();
		services.TryAddEnumerable(
			ServiceDescriptor.Singleton<
				IConfigureOptions<Microsoft.AspNetCore.Builder.RequestLocalizationOptions>,
				ConfigureRequestLocalizationOptions>());

		return services
			.AddOptions<LocanRequestLocalizationOptions>()
			.Validate(
				static value => !string.IsNullOrWhiteSpace(value.DefaultCulture),
				$"{nameof(LocanRequestLocalizationOptions.DefaultCulture)} cannot be empty.")
			.Validate(
				static value => value.SupportedCultures is not null &&
					value.SupportedCultures.All(static culture => !string.IsNullOrWhiteSpace(culture)),
				$"{nameof(LocanRequestLocalizationOptions.SupportedCultures)} contains an empty culture.")
			.Validate(
				static value => IsCulture(value.DefaultCulture) &&
					value.SupportedCultures is not null &&
					value.SupportedCultures.All(IsCulture),
				"Request localization contains an invalid culture.");
	}

	private static bool IsCulture(string? name)
	{
		if (string.IsNullOrWhiteSpace(name)) return false;

		try
		{
			_ = CultureInfo.GetCultureInfo(name);
			return true;
		}
		catch (CultureNotFoundException)
		{
			return false;
		}
	}
}
