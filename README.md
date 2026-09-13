# Locan

[![NuGet](https://img.shields.io/nuget/v/Locan.AspNetCore?label=NuGet)](https://www.nuget.org/packages/Locan.AspNetCore)

Locan is a strongly typed localization library for .NET. It keeps translations in JSON, generates message classes at compile time, and renders them using the current UI culture.

## Why Locan

- Message keys become discoverable C# types instead of string literals.
- Placeholder names, types, and formats are captured by the source generator.
- Culture-aware formatting uses standard .NET format strings.
- Runtime resources support parent-culture fallback such as `ru-RU` to `ru`.
- Hosting and ASP.NET Core integrations load resources and select the request culture.

## Installation

For an ASP.NET Core application with generated messages:

```shell
dotnet add package Locan.AspNetCore
dotnet add package Locan.Generator
```

## Two-minute example

Declare the default culture and localization resources in the project file:

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

Declare the namespace for generated message classes:

```csharp
using Locan.Core.Attributes;

[assembly: LocalizationModule("MyApplication.Messages")]
```

Create `Localization/localization-en.json`:

```json
{
  "culture": "en",
  "messages": {
    "article.not.found": "Article not found.",
    "article.price.updated": "Price updated to {Price|Decimal|F2}."
  }
}
```

The generator turns `article.price.updated` into a class with a typed factory:

```csharp
using Locan.Core.LocalizableMessages;

public partial class ArticlePriceUpdatedMessage : LocalizableMessage
{
    public const string Key = "article.price.updated";

    public ArticlePriceUpdatedMessage() : base(Key, 1) { }

    public static ArticlePriceUpdatedMessage Create(decimal Price)
    {
        var message = new ArticlePriceUpdatedMessage();
        message.WithValue("Price", Price, "F2");
        return message;
    }

    public ArticlePriceUpdatedMessage WithPrice(decimal value)
    {
        WithValue("Price", value, "F2");
        return this;
    }
}
```

Use the generated class through Locan's contextual localizer:

```csharp
using Locan.AspNetCore;
using Locan.Core.Interfaces.Localizers;
using MyApplication.Messages;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLocanAspNetCore(options =>
{
    options.DefaultCulture = "en";
    options.SupportedCultures = ["en", "ru"];
});

var app = builder.Build();
app.UseLocanRequestLocalization();

app.MapGet("/price", (IContextualLocalizer localizer) =>
    localizer.Get(ArticlePriceUpdatedMessage.Create(12.5m)));

app.Run();
```

For the English request culture, the result is `Price updated to 12.50.`. Resources with `CopyToOutput="true"` are copied to the application's `Locan` output directory automatically.

## Packages

- `Locan.Core` — shared contracts, models, and template parsing primitives.
- `Locan` — resource loading, culture lookup, and message rendering.
- `Locan.Hosting` — dependency injection and hosted initialization.
- `Locan.AspNetCore` — request-culture integration for ASP.NET Core.
- `Locan.Generator` — build integration and the strongly typed source generator.

## Documentation

- [Configuration and resources](docs/configuration.md)
- [Message templates](docs/templates.md)
- [Source generator](docs/source-generator.md)

## License

Locan is licensed under the [MIT License](https://github.com/UnidentifiedSheep/Locan/blob/master/LICENSE).
