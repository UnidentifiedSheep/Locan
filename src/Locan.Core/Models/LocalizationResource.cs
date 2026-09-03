using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Locan.Core.Models;

public sealed class LocalizationResource
{
	[JsonPropertyName("culture")]
	[Required]
	public required string Culture { get; init; }

	[JsonPropertyName("messages")]
	[Required]
	public required Dictionary<string, string> Messages { get; init; }
}
