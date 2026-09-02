using System.Globalization;

namespace Locan.Core.Interfaces.Containers;

public interface ILocalizerContainerProvider
{
	ILocalizerContainer? Find(CultureInfo culture);
	ILocalizerContainer GetRequired(CultureInfo culture);
}
