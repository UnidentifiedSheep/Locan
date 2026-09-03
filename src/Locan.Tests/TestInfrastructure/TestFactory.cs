using System.Globalization;
using Locan.Containers;
using Locan.Core.Interfaces.Containers;
using Locan.TemplateRenderers;

namespace Locan.Tests.TestInfrastructure;

internal static class TestFactory
{
	public const string DefaultMessageKey = "Message";

	public static SegmentedLocalizerContainer CreateContainer(
		string culture = "en",
		IReadOnlyDictionary<string, string>? messages = null) =>
		new(
			CultureInfo.GetCultureInfo(culture),
			messages ?? new Dictionary<string, string>());

	public static LocalizerContainerProvider CreateProvider(
		params ILocalizerContainer[] containers)
	{
		var provider = new LocalizerContainerProvider();
		provider.SetContainers(containers);
		return provider;
	}

	public static Localizer CreateLocalizer(
		params ILocalizerContainer[] containers) =>
		new(
			CreateProvider(containers),
			new SegmentedMessageTemplateRenderer());

	public static IMessageSegmentsContainer CreateTemplate(string template)
	{
		var container = new SegmentedLocalizerContainer(
			CultureInfo.InvariantCulture,
			new Dictionary<string, string>
			{
				[DefaultMessageKey] = template
			});

		return container[DefaultMessageKey];
	}
}
