# Configuration and resources

Create `localizationSettings.json` in the project directory to select localization files:

```json
{
  "defaultCulture": "en",
  "paths": [
    {
      "folderPath": "Localization",
      "searchPattern": "*.locan.json",
      "recursive": true
    }
  ]
}
```

- `folderPath` is relative to the settings file.
- `searchPattern` defaults to `*.json`.
- `recursive` defaults to `true`.
- `defaultCulture` selects the resources used to generate the C# API.

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

Discovered files are copied to the application's `Locan` output directory and loaded during startup. Files with the same culture are merged; duplicate keys within one culture fail initialization.

Set `"isTemplate": true` on a resource only when it should also participate in source generation despite not matching `defaultCulture`.

At runtime, culture lookup follows `CultureInfo.Parent`, for example `ru-RU → ru`. It does not automatically fall back to the `defaultCulture` from this file.
