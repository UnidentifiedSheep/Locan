using Microsoft.AspNetCore.Builder;

namespace Locan.AspNetCore;

public static class LocanApplicationBuilderExtensions
{
	public static IApplicationBuilder UseLocanRequestLocalization(
		this IApplicationBuilder application)
	{
		ArgumentNullException.ThrowIfNull(application);
		return application.UseRequestLocalization();
	}
}
