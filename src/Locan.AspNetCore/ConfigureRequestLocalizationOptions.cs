using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;

namespace Locan.AspNetCore;

internal sealed class ConfigureRequestLocalizationOptions(
	IOptions<LocanRequestLocalizationOptions> source) :
	IConfigureOptions<RequestLocalizationOptions>
{
	public void Configure(RequestLocalizationOptions options)
	{
		var configured = source.Value;
		var supportedCultureNames = configured.SupportedCultures
			.Prepend(configured.DefaultCulture)
			.Distinct(StringComparer.OrdinalIgnoreCase);
		var supportedCultures = supportedCultureNames
			.Select(CultureInfo.GetCultureInfo)
			.ToArray();

		options.DefaultRequestCulture = new RequestCulture(configured.DefaultCulture);
		options.SupportedCultures = supportedCultures;
		options.SupportedUICultures = supportedCultures;
	}
}
