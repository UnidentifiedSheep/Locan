# Locan

Locan is a strongly typed localization library for .NET with JSON resources and a Roslyn source generator.

## Packages

- `Locan.Core` — shared contracts and localization models.
- `Locan` — localization runtime.
- `Locan.Hosting` — dependency injection and hosted initialization.
- `Locan.AspNetCore` — ASP.NET Core integration.
- `Locan.Generator` — strongly typed message source generator.

## Installation

For an ASP.NET Core application with generated messages:

```shell
dotnet add package Locan.AspNetCore
dotnet add package Locan.Generator
```

## Source generator

Add `localizationSettings.json` to the project directory:

```json
{
  "defaultCulture": "en",
  "paths": [
    {
      "searchPattern": "localization-*.json",
      "folderPath": "Localization",
      "recursive": false
    }
  ]
}
```

Declare the namespace for generated message classes:

```csharp
using Locan.Generator.Attributes;

[assembly: LocalizationModule("MyApplication.Messages")]
```

Define the default-culture resource:

```json
{
  "culture": "en",
  "messages": {
    "article.not.found": "Article not found.",
    "article.price.updated": "Price updated to {Price|Decimal|F2}."
  }
}
```

The generator creates strongly typed classes such as `ArticleNotFoundMessage` and `ArticlePriceUpdatedMessage`.

## License

Locan is licensed under the [MIT License](https://github.com/UnidentifiedSheep/Locan/blob/master/LICENSE).
