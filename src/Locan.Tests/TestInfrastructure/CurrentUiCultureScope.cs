using System.Globalization;

namespace Locan.Tests.TestInfrastructure;

internal sealed class CurrentUiCultureScope : IDisposable
{
	private readonly CultureInfo _previous = CultureInfo.CurrentUICulture;

	public CurrentUiCultureScope(string culture)
	{
		CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
	}

	public void Dispose()
	{
		CultureInfo.CurrentUICulture = _previous;
	}
}
