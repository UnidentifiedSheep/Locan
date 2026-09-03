using Locan.LocalizableMessages;

namespace Locan.Tests;

public sealed class LocalizableMessageTests
{
	private readonly LocalizableMessage _message = new("Greeting");

	[Fact]
	public void Constructor_StoresMessageKey()
	{
		Assert.Equal("Greeting", _message.MessageKey);
		Assert.Empty(_message.Values);
	}

	[Fact]
	public void WithValue_ReturnsSameMessage()
	{
		var result = _message.WithValue("Name", "Alex");

		Assert.Same(_message, result);
	}

	[Fact]
	public void WithValue_StoresValue()
	{
		_message.WithValue("Name", "Alex");

		Assert.Equal("Alex", _message.Values["Name"]);
	}

	[Fact]
	public void WithValue_OverwritesExistingValue()
	{
		_message.WithValue("Name", "First");

		_message.WithValue("Name", "Second");

		Assert.Single(_message.Values);
		Assert.Equal("Second", _message.Values["Name"]);
	}

	[Fact]
	public void WithValue_AllowsNullValue()
	{
		_message.WithValue("Name", null);

		Assert.True(_message.Values.ContainsKey("Name"));
		Assert.Null(_message.Values["Name"]);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	public void Constructor_RejectsInvalidMessageKey(string? key)
		=> Assert.ThrowsAny<ArgumentException>(() => new LocalizableMessage(key!));

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	public void WithValue_RejectsInvalidKey(string? key)
	{
		Assert.ThrowsAny<ArgumentException>(() => _message.WithValue(key!, "Value"));
	}
}
