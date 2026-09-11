using System.Text.Json;
using Locan.Compiler.Models;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Task = Microsoft.Build.Utilities.Task;

namespace Locan.Build.Build;

public sealed class ResolveLocalizationFiles : Task
{
    [Required]
    public string ConfigPath { get; set; } = null!;

    [Output]
    public ITaskItem[] Files { get; set; } = [];

    public override bool Execute()
    {
        if (!File.Exists(ConfigPath))
        {
            Log.LogError($"Locan config not found: {ConfigPath}");
            return false;
        }

        try
        {
            var configDirectory = Path.GetDirectoryName(
                Path.GetFullPath(ConfigPath))!;

            var json = File.ReadAllText(ConfigPath);

            var config =
                JsonSerializer.Deserialize<LocalizationGenerationOptions>(json);

            if (config is null)
            {
                Log.LogError("Failed to deserialize Locan config.");
                return false;
            }

            var files = config.Paths
                .SelectMany(options =>
                {
                    var directory = Path.IsPathRooted(options.FolderPath)
                        ? Path.GetFullPath(options.FolderPath)
                        : Path.GetFullPath(options.FolderPath, configDirectory);

                    var searchOption = options.Recursive
                        ? SearchOption.AllDirectories
                        : SearchOption.TopDirectoryOnly;

                    return Directory.EnumerateFiles(
                        directory,
                        options.SearchPattern,
                        searchOption);
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            Files = files
                .GroupBy(
                    Path.GetFileName,
                    StringComparer.OrdinalIgnoreCase)
                .SelectMany(group =>
                {
                    var groupedFiles = group.ToArray();

                    return groupedFiles.Length == 1
						? [GenItem(groupedFiles[0], null)]
						: GenerateUniqueItems(groupedFiles);
				})
                .ToArray();

            return true;
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }
    }

    private static IReadOnlyCollection<ITaskItem> GenerateUniqueItems(
        IReadOnlyList<string> files)
    {
        var directories = files
            .Select(file => Path.GetDirectoryName(file)!)
            .ToArray();

        var segments = directories
            .Select(SplitPath)
            .ToArray();

        for (var depth = 1; ; depth++)
        {
            var candidates = segments
                .Select(parts => TakeLast(parts, depth))
                .ToArray();

            if (candidates.Distinct(
                    StringComparer.OrdinalIgnoreCase).Count() != files.Count)
            {
                continue;
            }

            return files
                .Select((file, index) =>
                    GenItem(file, candidates[index]))
                .ToArray();
        }
    }

    private static string[] SplitPath(string path)
    {
        var root = Path.GetPathRoot(path);

        var relative = root is null
            ? path
            : path[root.Length..];

        return relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static string TakeLast(
        IReadOnlyList<string> parts,
        int count)
    {
        var skip = Math.Max(0, parts.Count - count);

        return Path.Combine(
            parts.Skip(skip).ToArray());
    }

    private static ITaskItem GenItem(
        string filePath,
        string? moduleName)
    {
        var item = new TaskItem(filePath);

        var targetPath = string.IsNullOrWhiteSpace(moduleName)
            ? Path.Combine("Locan", Path.GetFileName(filePath))
            : Path.Combine(
                "Locan",
                moduleName,
                Path.GetFileName(filePath));

        item.SetMetadata("TargetPath", targetPath);

        return item;
    }
}
