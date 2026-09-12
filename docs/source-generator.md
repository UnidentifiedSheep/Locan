# Source generator

The generator turns localization keys into strongly typed message classes. Add the assembly attribute to select their namespace:

```csharp
using Locan.Generator.Attributes;

[assembly: LocalizationModule("MyApplication.Messages")]
```

Set `IsInternal = true` if the generated classes should not be public.

## Generated names

Locan splits a key on non-letter and non-digit characters, joins the parts, and appends `Message`:

| Key | Class |
| --- | --- |
| `article.not.found` | `ArticleNotFoundMessage` |
| `article-price-updated` | `ArticlePriceUpdatedMessage` |
| `404.title` | `_404TitleMessage` |

Keys such as `article.price` and `article-price` conflict because they produce the same class name.

## Generated API

For this template:

```text
"article.price.updated": "Article {Sku|string} now costs {Price|decimal|F2}."
```

the generated class provides a key, an empty constructor, a typed factory, and fluent placeholder methods:

```csharp
var complete = ArticlePriceUpdatedMessage.Create("SKU-42", 12.5m);

var incremental = new ArticlePriceUpdatedMessage()
    .WithSku("SKU-42")
    .WithPrice(12.5m);

var text = contextualLocalizer.Get(complete);
```

The factory parameters and `With...` methods use the types and formats declared in the canonical template. See [message templates](templates.md) for the supported syntax.

## Multiple locales

Within paths where `generateMessages` is enabled, resources matching `defaultCulture`, plus resources marked with `isTemplate: true`, participate in generation. Other locales are not checked by the generator; `copyToOutput` independently controls whether any matched resource is copied for runtime use.

Canonical files may contribute different keys. If the same key occurs more than once, its placeholder names, types, nullability, and formats must match. Missing keys in ordinary translation files are not currently reported at compile time.

## Diagnostics

The generator currently reports errors for invalid module names or resources, invalid message keys, conflicting generated class names, and incompatible placeholder contracts (`LOCAN001`–`LOCAN005`).
