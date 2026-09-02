using System.Globalization;

namespace Locan.Core.Interfaces.Containers;

public interface ILocalizerContainer : IReadOnlyDictionary<string, IMessageSegmentsContainer>
{
	CultureInfo Locale { get; }
	void Initialize(IReadOnlyDictionary<string, string> keyMessages);
}
