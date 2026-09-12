namespace Locan.Core.Models;

public readonly record struct LocalizableMessageValue(
	object? Value,
	string? Format);
