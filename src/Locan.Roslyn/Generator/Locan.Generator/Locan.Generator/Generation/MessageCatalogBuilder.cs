using System;
using System.Collections.Generic;
using Locan.Core.Exceptions;
using Locan.Generator.Diagnostics;
using Locan.Generator.Models;
using Locan.Generator.Parsing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Locan.Generator.Generation;

internal sealed class MessageCatalogBuilder
{
	private readonly Dictionary<string, MessageDefinition> _definitions = new(StringComparer.Ordinal);
	private readonly List<Diagnostic> _diagnostics = [];

	public IEnumerable<MessageDefinition> Definitions => _definitions.Values;

	public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

	public void Add(string key, string template)
	{
		var className = CSharpNames.NormalizeClassName(key) + "Message";

		if (!SyntaxFacts.IsValidIdentifier(className))
		{
			_diagnostics.Add(Diagnostic.Create(
				GeneratorDiagnosticDescriptors.InvalidMessageKey,
				Location.None,
				key));
			return;
		}

		IReadOnlyDictionary<string, PlaceholderInfo> placeholders;

		try
		{
			placeholders = PlaceholderParser.Parse(template);
		}
		catch (MessageTemplateParseException exception)
		{
			_diagnostics.Add(Diagnostic.Create(
				GeneratorDiagnosticDescriptors.InvalidLocalizationFile,
				Location.None,
				key,
				exception.Message));
			return;
		}

		if (!_definitions.TryGetValue(className, out var existing))
		{
			_definitions.Add(
				className,
				new MessageDefinition(key, className, template, placeholders));
			return;
		}

		if (!string.Equals(existing.Key, key, StringComparison.Ordinal))
		{
			_diagnostics.Add(Diagnostic.Create(
				GeneratorDiagnosticDescriptors.ConflictingClassName,
				Location.None,
				existing.Key,
				key,
				className));
			_definitions.Remove(className);
			return;
		}

		if (PlaceholderParser.AreEquivalent(existing.Placeholders, placeholders))
			return;

		_diagnostics.Add(Diagnostic.Create(
			GeneratorDiagnosticDescriptors.ConflictingTemplate,
			Location.None,
			key));
		_definitions.Remove(className);
	}
}
