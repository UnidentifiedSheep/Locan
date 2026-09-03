namespace Locan.Core.Exceptions;

public sealed class PlaceholderValueNotFoundException : KeyNotFoundException
{
	public PlaceholderValueNotFoundException(string placeholderKey)
		: base(CreateMessage(placeholderKey))
	{
		PlaceholderKey = placeholderKey;
		Data.Add(nameof(placeholderKey), placeholderKey);
	}

	public string PlaceholderKey { get; }

	private static string CreateMessage(string placeholderKey)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(placeholderKey);
		return $"Value for placeholder '{placeholderKey}' was not found.";
	}
}
