# Configuration and resources

Locan discovers resources through declarative MSBuild items. Because these items exist during project evaluation, the CLI, Rider, and Visual Studio see the same source-generator inputs.

## Build-time configuration

Add the default culture and resource glob to the project file:

```xml
<PropertyGroup>
  <LocanDefaultCulture>en</LocanDefaultCulture>
</PropertyGroup>

<ItemGroup>
  <LocanResource Include="Localization/**/*.json"
                 GenerateMessages="true"
                 CopyToOutput="true" />
</ItemGroup>
```

`LocanDefaultCulture` defaults to `en`. The generator reads all resources with `GenerateMessages="true"` and uses resources whose `culture` matches this property, plus resources marked with `"isTemplate": true`.

Each `LocanResource` supports two independent options, both defaulting to `true`:

| `GenerateMessages` | `CopyToOutput` | Result |
| --- | --- | --- |
| `true` | `true` | Use for generation and copy for runtime |
| `true` | `false` | Use for generation only |
| `false` | `true` | Copy for runtime only |
| `false` | `false` | Keep only as a project item |

Separate globs can be used when source templates and deployed translations live in different directories:

```xml
<ItemGroup>
  <LocanResource Include="Localization/Templates/**/*.json"
                 GenerateMessages="true"
                 CopyToOutput="false" />
  <LocanResource Include="Localization/Translations/**/*.json"
                 GenerateMessages="false"
                 CopyToOutput="true" />
</ItemGroup>
```

## IDE resource layout

By default, Locan resources appear in Rider and Visual Studio under a virtual `Locan` folder. The recursive part of each resource glob is preserved:

```xml
<LocanResource Include="../Main.Entities/Localization/**/*.json"
               GenerateMessages="false"
               CopyToOutput="true" />
```

For example, `../Main.Entities/Localization/Errors/validation.json` is displayed as `Locan/Errors/validation.json`. This only changes the IDE presentation: the source file stays in its original location, and build and publish output paths are still controlled independently by Locan.

Set `Link` explicitly to choose another virtual path. Locan does not overwrite a user-defined value:

```xml
<LocanResource Include="../Main.Entities/Localization/**/*.json">
  <Link>SharedResources/%(RecursiveDir)%(Filename)%(Extension)</Link>
</LocanResource>
```

Automatic links can be disabled for the whole project:

```xml
<PropertyGroup>
  <LocanUseDefaultResourceLink>false</LocanUseDefaultResourceLink>
</PropertyGroup>
```

## Resource format

Each resource contains a culture and a message dictionary:

```json
{
  "culture": "en",
  "messages": {
    "article.not.found": "Article not found.",
    "article.price.updated": "Price updated to {Price|decimal|F2}."
  }
}
```

Translations use the same keys and placeholder names:

```json
{
  "culture": "ru",
  "messages": {
    "article.not.found": "Артикул не найден.",
    "article.price.updated": "Цена обновлена: {Price}."
  }
}
```

Resources selected for copying are placed under `Locan/` in build and publish output, preserving the recursive part of the glob. They are loaded during application startup. The same key appearing in multiple files for one culture fails initialization.

At runtime, culture lookup follows `CultureInfo.Parent`, for example `ru-RU → ru`. It does not automatically fall back to `LocanDefaultCulture`.
