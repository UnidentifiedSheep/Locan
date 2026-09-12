# Configuration and resources

Create `localizationSettings.json` in the project directory to select localization files:

```json
{
  "defaultCulture": "en",
  "paths": [
    {
      "folderPath": "Localization",
      "searchPattern": "*.locan.json",
      "recursive": true,
      "generateMessages": true,
      "copyToOutput": true
    }
  ]
}
```

- `folderPath` is relative to the settings file.
- `searchPattern` defaults to `*.json`.
- `recursive` defaults to `true`.
- `generateMessages` controls whether matching canonical resources participate in source generation. It defaults to `true`.
- `copyToOutput` controls whether matching resources are copied for runtime loading. It defaults to `true`.
- `defaultCulture` selects the resources used to generate the C# API.

The two operations are independent:

| `generateMessages` | `copyToOutput` | Result |
| --- | --- | --- |
| `true` | `true` | Generate messages and copy resources |
| `true` | `false` | Generate messages only |
| `false` | `true` | Copy resources only |
| `false` | `false` | Ignore the path |

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

Additional locales use the same keys:

```json
{
  "culture": "ru",
  "messages": {
    "article.not.found": "Артикул не найден.",
    "article.price.updated": "Цена обновлена: {Price}."
  }
}
```

Resources selected for copying are placed in the application's `Locan` output directory and loaded during startup. Files with the same culture are merged; the same key appearing in multiple files for one culture fails initialization.

Set `"isTemplate": true` on a resource only when it should also participate in source generation despite not matching `defaultCulture`.

At runtime, culture lookup follows `CultureInfo.Parent`, for example `ru-RU → ru`. It does not automatically fall back to the `defaultCulture` from this file.
