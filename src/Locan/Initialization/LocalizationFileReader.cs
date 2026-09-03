using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json;
using Locan.Core.Models;

namespace Locan.Initialization;

internal static class LocalizationFileReader
{
	private static readonly JsonSerializerOptions SerializerOptions = new()
	{
		RespectNullableAnnotations = true
	};

	public static async Task<(CultureInfo Culture, Dictionary<string, string> Messages)> ReadAsync(
		string filePath,
		CancellationToken cancellationToken)
	{
		LocalizationResource? resource;

		try
		{
			await using var stream = File.OpenRead(filePath);
			resource = await JsonSerializer.DeserializeAsync<LocalizationResource>(
				stream,
				SerializerOptions,
				cancellationToken: cancellationToken);
		}
		catch (JsonException exception)
		{
			throw new InvalidDataException(
				$"Localization file '{filePath}' contains invalid JSON.",
				exception);
		}

		if (resource is null)
			throw new InvalidDataException($"Localization file '{filePath}' is empty.");

		try
		{
			Validator.ValidateObject(
				resource,
				new ValidationContext(resource),
				validateAllProperties: true);
		}
		catch (ValidationException exception)
		{
			throw new InvalidDataException(
				$"Localization file '{filePath}' has invalid data.",
				exception);
		}

		try
		{
			return (CultureInfo.GetCultureInfo(resource.Culture), resource.Messages);
		}
		catch (CultureNotFoundException exception)
		{
			throw new InvalidDataException(
				$"Localization file '{filePath}' contains invalid culture '{resource.Culture}'.",
				exception);
		}
	}
}
