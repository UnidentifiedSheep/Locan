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

    [Output]
    public ITaskItem[] TemplateFiles { get; set; } = [];

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

            if (string.IsNullOrWhiteSpace(config.DefaultCulture))
            {
                Log.LogError("Locan default culture cannot be empty.");
                return false;
            }

            var fullConfigPath = Path.GetFullPath(ConfigPath);

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
                .Where(file => !string.Equals(
                    Path.GetFullPath(file),
                    fullConfigPath,
                    StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var templateFiles = files
                .Where(file => IsTemplate(file, config.DefaultCulture))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

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

            TemplateFiles = Files
                .Where(file => templateFiles.Contains(file.ItemSpec))
                .ToArray();

            return true;
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }
    }

    private static bool IsTemplate(
        string filePath,
        string defaultCulture)
    {
        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(filePath));
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                throw new InvalidDataException("The root JSON value must be an object.");

            if (!root.TryGetProperty("culture", out var cultureElement) ||
                cultureElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(cultureElement.GetString()))
            {
                throw new InvalidDataException(
                    "The localization file must contain a non-empty string property 'culture'.");
            }

            var isTemplate = false;

            if (root.TryGetProperty("isTemplate", out var isTemplateElement))
            {
                if (isTemplateElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                {
                    throw new InvalidDataException(
                        "The localization property 'isTemplate' must be a boolean.");
                }

                isTemplate = isTemplateElement.GetBoolean();
            }

            return isTemplate || string.Equals(
                cultureElement.GetString(),
                defaultCulture,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Localization file '{filePath}' contains invalid JSON.",
                exception);
        }
        catch (InvalidDataException exception)
        {
            throw new InvalidDataException(
                $"Localization file '{filePath}' is invalid: {exception.Message}",
                exception);
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

            if (candidates.Distinct(StringComparer.OrdinalIgnoreCase).Count() != files.Count)
				continue;

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
