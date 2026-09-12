namespace Locan.Generator.Models;

internal sealed class PlaceholderInfo
{
	public PlaceholderInfo(string key)
	{
		Key = key;
	}

	public string Key { get; }

	public string? Type { get; set; }

	public string? Format { get; set; }
}
