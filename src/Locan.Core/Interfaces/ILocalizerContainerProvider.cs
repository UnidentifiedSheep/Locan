using System.Globalization;

namespace Locan.Core.Interfaces;

public interface ILocalizerContainerProvider
{
	ILocalizerContainer? Find(CultureInfo culture);
	ILocalizerContainer? TryGetRequired(CultureInfo culture);
}
