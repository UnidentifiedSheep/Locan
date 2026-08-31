using System.Globalization;

namespace Locan.Core.Interfaces;

public interface ILocalizerContainer : IReadOnlyDictionary<string, string>
{
	CultureInfo Locale { get; }
	void Initialize(Dictionary<string, string> keyMessages);
}
