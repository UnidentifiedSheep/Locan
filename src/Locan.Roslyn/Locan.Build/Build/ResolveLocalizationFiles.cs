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

            var resolvedFiles = ResolveFiles(
                config.Paths,
                configDirectory,
                fullConfigPath);
            var selectedFiles = resolvedFiles.Values
                .Where(static file => file.GenerateMessages || file.CopyToOutput)
                .OrderBy(static file => file.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var items = GenerateItems(selectedFiles.Select(static file => file.Path));

            Files = selectedFiles
                .Where(static file => file.CopyToOutput)
                .Select(file => items[file.Path])
                .ToArray();

            TemplateFiles = selectedFiles
                .Where(file => file.GenerateMessages && IsTemplate(file.Path, config.DefaultCulture))
                .Select(file => items[file.Path])
                .ToArray();

            return true;
        }
        catch (Exception exception)
        {
            Log.LogErrorFromException(exception);
            return false;
        }
    }

    private static Dictionary<string, ResolvedFile> ResolveFiles(
        IEnumerable<LocalizationGenerationPathOptions> paths,
        string configDirectory,
        string fullConfigPath)
    {
        var result = new Dictionary<string, ResolvedFile>(StringComparer.OrdinalIgnoreCase);

        foreach (var options in paths)
        {
            if (!options.GenerateMessages && !options.CopyToOutput)
                continue;

            var directory = Path.GetFullPath(
                Path.IsPathRooted(options.FolderPath)
                    ? options.FolderPath
                    : Path.Combine(configDirectory, options.FolderPath));
            var searchOption = options.Recursive
                ? SearchOption.AllDirectories
                : SearchOption.TopDirectoryOnly;

            foreach (var path in Directory.EnumerateFiles(
                         directory,
                         options.SearchPattern,
                         searchOption))
            {
                var fullPath = Path.GetFullPath(path);

                if (string.Equals(
                        fullPath,
                        fullConfigPath,
                        StringComparison.OrdinalIgnoreCase))
                    continue;

                if (result.TryGetValue(fullPath, out var existing))
                {
                    result[fullPath] = existing with
                    {
                        GenerateMessages = existing.GenerateMessages || options.GenerateMessages,
                        CopyToOutput = existing.CopyToOutput || options.CopyToOutput
                    };
                    continue;
                }

                result.Add(
                    fullPath,
                    new ResolvedFile(
                        fullPath,
                        options.GenerateMessages,
                        options.CopyToOutput));
            }
        }

        return result;
    }

    private static Dictionary<string, ITaskItem> GenerateItems(IEnumerable<string> files)
    {
        return files
            .GroupBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .SelectMany(group =>
            {
                var groupedFiles = group.ToArray();

                return groupedFiles.Length == 1
                    ? [GenItem(groupedFiles[0], null)]
                    : GenerateUniqueItems(groupedFiles);
            })
            .ToDictionary(
                static item => item.ItemSpec,
                StringComparer.OrdinalIgnoreCase);
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
				throw new InvalidDataException(
					"The localization file must contain a non-empty string property 'culture'.");

			var isTemplate = false;

            if (root.TryGetProperty("isTemplate", out var isTemplateElement))
            {
                if (isTemplateElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
					throw new InvalidDataException("The localization property 'isTemplate' must be a boolean.");

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
        string[] files)
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

            if (candidates.Distinct(StringComparer.OrdinalIgnoreCase).Count() != files.Length)
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
		var relative = root is null ? path : path.Substring(root.Length);

        return relative.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static string TakeLast(string[] parts, int count)
    {
        var skip = Math.Max(0, parts.Length - count);
        return Path.Combine(parts.Skip(skip).ToArray());
    }

    private static TaskItem GenItem(string filePath, string? moduleName)
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

    private sealed record ResolvedFile(
        string Path,
        bool GenerateMessages,
        bool CopyToOutput);
}
