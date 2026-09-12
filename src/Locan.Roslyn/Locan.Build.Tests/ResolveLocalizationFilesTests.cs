using System.Text.Json;
using Locan.Build.Build;
using Locan.Compiler.Models;
using Xunit;

namespace Locan.Build.Tests;

public sealed class ResolveLocalizationFilesTests
{
    [Fact]
    public void Execute_DefaultFlagsPreserveExistingBehavior()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteResource("messages.en.json", "en");
        directory.WriteResource("messages.ru.json", "ru");
        var task = directory.CreateTask($$"""
            {
              "defaultCulture": "en",
              "paths": [
                {
                  "folderPath": {{JsonSerializer.Serialize(directory.Path)}}
                }
              ]
            }
            """);

        Assert.True(task.Execute());

        Assert.Equal(2, task.Files.Length);
        Assert.Equal("messages.en.json", Path.GetFileName(Assert.Single(task.TemplateFiles).ItemSpec));
    }

    [Theory]
    [InlineData(true, true, 1, 1)]
    [InlineData(true, false, 0, 1)]
    [InlineData(false, true, 1, 0)]
    [InlineData(false, false, 0, 0)]
    public void Execute_AppliesPathFlagsIndependently(
        bool generateMessages,
        bool copyToOutput,
        int expectedFiles,
        int expectedTemplateFiles)
    {
        using var directory = new TemporaryDirectory();
        directory.WriteResource("messages.en.json", "en");
        var task = directory.CreateTask(new LocalizationGenerationPathOptions
        {
            FolderPath = directory.Path,
            GenerateMessages = generateMessages,
            CopyToOutput = copyToOutput
        });

        Assert.True(task.Execute());

        Assert.Equal(expectedFiles, task.Files.Length);
        Assert.Equal(expectedTemplateFiles, task.TemplateFiles.Length);
    }

    [Fact]
    public void Execute_CopyOnlyDoesNotParseResourceForGeneration()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteFile("invalid.json", "not json");
        var task = directory.CreateTask(new LocalizationGenerationPathOptions
        {
            FolderPath = directory.Path,
            GenerateMessages = false,
            CopyToOutput = true
        });

        Assert.True(task.Execute());

        Assert.Single(task.Files);
        Assert.Empty(task.TemplateFiles);
    }

    [Fact]
    public void Execute_IsTemplateGeneratesForNonDefaultCulture()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteResource("messages.ru.json", "ru", isTemplate: true);
        var task = directory.CreateTask(new LocalizationGenerationPathOptions
        {
            FolderPath = directory.Path,
            GenerateMessages = true,
            CopyToOutput = false
        });

        Assert.True(task.Execute());

        Assert.Empty(task.Files);
        Assert.Single(task.TemplateFiles);
    }

    [Fact]
    public void Execute_OverlappingPathsCombineFlags()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteResource("messages.en.json", "en");
        var task = directory.CreateTask(
            new LocalizationGenerationPathOptions
            {
                FolderPath = directory.Path,
                GenerateMessages = true,
                CopyToOutput = false
            },
            new LocalizationGenerationPathOptions
            {
                FolderPath = directory.Path,
                GenerateMessages = false,
                CopyToOutput = true
            });

        Assert.True(task.Execute());

        Assert.Single(task.Files);
        Assert.Single(task.TemplateFiles);
        Assert.Equal(task.Files[0].ItemSpec, task.TemplateFiles[0].ItemSpec);
    }

    [Fact]
    public void Execute_DisabledPathDoesNotRequireDirectory()
    {
        using var directory = new TemporaryDirectory();
        var task = directory.CreateTask(new LocalizationGenerationPathOptions
        {
            FolderPath = Path.Combine(directory.Path, "missing"),
            GenerateMessages = false,
            CopyToOutput = false
        });

        Assert.True(task.Execute());

        Assert.Empty(task.Files);
        Assert.Empty(task.TemplateFiles);
    }

    [Fact]
    public void Execute_DuplicateFileNamesReceiveUniqueTargetPaths()
    {
        using var directory = new TemporaryDirectory();
        directory.WriteResource("Articles/messages.json", "en");
        directory.WriteResource("Billing/messages.json", "en");
        var task = directory.CreateTask(new LocalizationGenerationPathOptions
        {
            FolderPath = directory.Path,
            Recursive = true
        });

        Assert.True(task.Execute());

        var targetPaths = task.Files
            .Select(static file => file.GetMetadata("TargetPath"))
            .ToArray();
        Assert.Equal(2, targetPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.All(targetPaths, static path => Assert.StartsWith($"Locan{Path.DirectorySeparatorChar}", path));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            $"Locan.Build.Tests.{Guid.NewGuid():N}");

        public TemporaryDirectory()
		{
			Directory.CreateDirectory(Path);
		}

		public ResolveLocalizationFiles CreateTask(
            params LocalizationGenerationPathOptions[] paths)
        {
            var config = new LocalizationGenerationOptions
            {
                DefaultCulture = "en",
                Paths = paths
            };
            return CreateTask(JsonSerializer.Serialize(config));
        }

        public ResolveLocalizationFiles CreateTask(string json)
        {
            var configPath = System.IO.Path.Combine(Path, "localizationSettings.json");
            File.WriteAllText(configPath, json);
            return new ResolveLocalizationFiles { ConfigPath = configPath };
        }

        public void WriteResource(string relativePath, string culture, bool isTemplate = false)
        {
            WriteFile(
                relativePath,
                JsonSerializer.Serialize(new
                {
                    culture,
                    isTemplate,
                    messages = new Dictionary<string, string>
                    {
                        ["message"] = "Message"
                    }
                }));
        }

        public void WriteFile(string relativePath, string content)
        {
            var filePath = System.IO.Path.Combine(Path, relativePath);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath)!);
            File.WriteAllText(filePath, content);
        }

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
