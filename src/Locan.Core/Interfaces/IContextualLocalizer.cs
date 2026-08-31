using System.Diagnostics.CodeAnalysis;

namespace Locan.Core.Interfaces;

public interface IContextualLocalizer
{
	string Get(ILocalizableMessage message);

	bool TryGet(
		ILocalizableMessage message,
		[NotNullWhen(true)]
		out string? value);
}
