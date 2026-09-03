namespace Locan.Tests.TestInfrastructure;

internal sealed class TemporaryDirectory : IDisposable
{
	public TemporaryDirectory()
	{
		Path = Directory.CreateTempSubdirectory("Locan.Tests.").FullName;
	}

	public string Path { get; }

	public async Task WriteFileAsync(string relativePath, string content)
	{
		var filePath = System.IO.Path.Combine(Path, relativePath);
		var directory = System.IO.Path.GetDirectoryName(filePath)!;
		Directory.CreateDirectory(directory);
		await File.WriteAllTextAsync(filePath, content);
	}

	public void Dispose()
	{
		if (Directory.Exists(Path))
			Directory.Delete(Path, recursive: true);
	}
}
