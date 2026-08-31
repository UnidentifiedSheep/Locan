using System.Globalization;

namespace Locan.Core.Interfaces;

public interface ILocalizerContainer
{
	CultureInfo Locale { get; }

	IReadOnlyDictionary<string, string> KeyMessages { get; }

	void Initialize(Dictionary<string, string> keyMessages);
}
