namespace Locan.Core.Compatibility;

internal static class ArgumentGuard
{
	public static void NotNullOrWhiteSpace(string? value, string parameterName)
	{
		if (value is null)
			throw new ArgumentNullException(parameterName);

		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException("Value cannot be empty or whitespace.", parameterName);
	}
}
