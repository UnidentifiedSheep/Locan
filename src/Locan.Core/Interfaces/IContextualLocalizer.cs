using System.Diagnostics.CodeAnalysis;
using Locan.Core.Models;

namespace Locan.Core.Interfaces;

public interface IContextualLocalizer
{
	string Get(LocalizableMessage message);

	bool TryGet(
		LocalizableMessage message,
		[NotNullWhen(true)]
		out string? value);
}
