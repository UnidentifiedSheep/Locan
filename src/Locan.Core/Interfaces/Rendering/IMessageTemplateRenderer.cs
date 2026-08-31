using System.Diagnostics.CodeAnalysis;
using Locan.Core.Interfaces.Containers;

namespace Locan.Core.Interfaces.Rendering;

public interface IMessageTemplateRenderer
{
	bool TryRender(
		IMessageSegmentsContainer segmentsContainer,
		ILocalizableMessage message,
		[NotNullWhen(true)] out string? rendered);

	string Render(IMessageSegmentsContainer segmentsContainer, ILocalizableMessage message);
}
