namespace Locan.Hosting;

public sealed class LocanOptions
{
	public const string SectionName = "Locan";

	public string DirectoryPath { get; set; } = Path.Combine(AppContext.BaseDirectory, SectionName);

	public string SearchPattern { get; set; } = "*.json";

	public bool Recursive { get; set; } = true;
}
