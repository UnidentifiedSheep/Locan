using System.Text.Json.Serialization;

namespace Locan.Compiler.Models;

public record LocalizationGenerationOptions
{
	[JsonPropertyName("defaultCulture")]
	public string DefaultCulture { get; set; } = "en";

	[JsonPropertyName("paths")]
	public IReadOnlyCollection<LocalizationGenerationPathOptions> Paths { get; set; } = [];
}

public record LocalizationGenerationPathOptions
{
	[JsonPropertyName("folderPath")]
	public required string FolderPath { get; set; }

	[JsonPropertyName("searchPattern")]
	public string SearchPattern { get; set; } = "*.json";

	[JsonPropertyName("recursive")]
	public bool Recursive { get; init; } = true;
}
