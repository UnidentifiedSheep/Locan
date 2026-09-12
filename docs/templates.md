# Message templates

Templates combine text with placeholders. A placeholder has up to three pipe-separated parts:

```text
{Name}
{Price|Decimal}
{Price|Decimal|F2}
```

The general form is `{Name|Type|Format}`. A format requires a type part; `{Price||F2}` is invalid.

## Placeholders

Names must begin with an ASCII letter or `_`, followed by ASCII letters, digits, or `_`. Names are case-sensitive, so `{Name}` and `{name}` are different values.

When the same placeholder occurs more than once in one template, its type and format metadata must match exactly:

```text
{Price|Decimal|F2}, previously {Price|Decimal|F2}
```

## Supported generated types

Both C# type keywords and their CLR names are supported. Matching is case-insensitive, so `int`, `Int`, and `INT` are equivalent.

| Template keyword | Generated type |
| --- | --- |
| `bool` / `Boolean` | `bool` |
| `byte` / `Byte` | `byte` |
| `sbyte` / `SByte` | `sbyte` |
| `short` / `Int16` | `short` |
| `ushort` / `UInt16` | `ushort` |
| `int` / `Int32` | `int` |
| `uint` / `UInt32` | `uint` |
| `long` / `Int64` | `long` |
| `ulong` / `UInt64` | `ulong` |
| `float` / `Single` | `float` |
| `double` / `Double` | `double` |
| `decimal` / `Decimal` | `decimal` |
| `char` / `Char` | `char` |
| `string` / `String` | `string` |
| `object` / `Object` | `object` |
| `DateTime` | `System.DateTime` |

For example, these declarations generate `int`, `long`, `bool`, and `string` parameters respectively:

```text
{Count|int}
{OrderId|Int64}
{IsActive|bool}
{Name|string}
```

If the type is omitted, the generator uses `string`. An unrecognized but syntactically valid identifier also currently falls back to `string`; it does not generate a custom C# type.

Append `?` to any supported name to generate a nullable type, for example `{Count|int?}`, `{Count|Int32?}`, or `{Text|string?}`. Fully qualified names such as `System.Int32` and `System.DateTime` are not accepted because dots are not valid inside the type part. Types such as `nint`, `nuint`, `Half`, `DateOnly`, `TimeOnly`, and `Guid` are not currently mapped and therefore fall back to `string`.

## Formats

Formats are standard .NET format strings passed to `IFormattable.ToString(format, culture)`:

```json
{
  "messages": {
    "price": "Price: {Price|Decimal|F2}",
    "count": "Processed: {Count|Int32|N0}",
    "completed": "Completed: {Date|DateTime|yyyy-MM-dd HH:mm:ss}"
  }
}
```

Formatting uses the culture requested from `ILocalizer`, or `CurrentUICulture` when using `IContextualLocalizer`. Values that do not implement `IFormattable` use `ToString()`.

The generated message stores the format from the canonical template. At runtime, translated templates use placeholder names; their type and format annotations are parsed for validity but do not override the generated value metadata.

## Literal braces

Double braces emit literal braces:

```text
Object: {{ Name: {Name} }}
```

renders as `Object: { Name: Alex }` when `Name` is `Alex`. An unpaired `}` or an unclosed/nested placeholder is invalid.

## Missing and null values

A missing placeholder value makes `TryGet` return `false` and makes `Get` throw `PlaceholderValueNotFoundException`. A supplied `null` value renders as empty text.
