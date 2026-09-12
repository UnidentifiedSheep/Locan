using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Locan.Generator.Tests.Utils;

internal sealed class TestAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
{
	private static readonly AnalyzerConfigOptions Empty = new TestAnalyzerConfigOptions(
		new Dictionary<string, string>());
	private readonly AnalyzerConfigOptions _globalOptions;

	public TestAnalyzerConfigOptionsProvider(IReadOnlyDictionary<string, string> globalOptions)
	{
		_globalOptions = new TestAnalyzerConfigOptions(globalOptions);
	}

	public override AnalyzerConfigOptions GlobalOptions => _globalOptions;

	public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => Empty;

	public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => Empty;

	private sealed class TestAnalyzerConfigOptions(
		IReadOnlyDictionary<string, string> values) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value) =>
			values.TryGetValue(key, out value!);
	}
}
