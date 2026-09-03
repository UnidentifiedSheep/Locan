namespace Locan.AspNetCore;

public sealed class LocanRequestLocalizationOptions
{
	public const string SectionName = "RequestLocalization";

	public string DefaultCulture { get; set; } = "en";

	public string[] SupportedCultures { get; set; } = [];
}
