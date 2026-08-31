using System.Diagnostics.CodeAnalysis;

namespace Locan.Core.Interfaces;

public interface IMessageTemplateRenderer
{
	bool TryRender(
		string template,
		ILocalizableMessage message,
		[NotNullWhen(true)] out string? rendered);

	string Render(string template, ILocalizableMessage message);
}
