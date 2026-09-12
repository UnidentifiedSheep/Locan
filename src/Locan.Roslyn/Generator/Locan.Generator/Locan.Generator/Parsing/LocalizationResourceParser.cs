using System;
using System.Text.Json;
using System.Threading;
using Locan.Core.Models;
using Microsoft.CodeAnalysis;

namespace Locan.Generator.Parsing;

internal static class LocalizationResourceParser
{
	public static bool TryParse(
		AdditionalText file,
		CancellationToken cancellationToken,
		out LocalizationResource? resource,
		out string? error)
	{
		try
		{
			var text = file.GetText(cancellationToken)?.ToString();

			if (text is null)
				throw new InvalidOperationException("the file could not be read");

			resource = JsonSerializer.Deserialize<LocalizationResource>(text);

			if (resource is null)
				throw new InvalidOperationException("the file is empty");

			if (resource.Messages is null)
				throw new InvalidOperationException("property 'messages' is required");

			error = null;
			return true;
		}
		catch (Exception exception) when (exception is JsonException or InvalidOperationException)
		{
			resource = null;
			error = exception.Message;
			return false;
		}
	}
}
