namespace Locan.Core.Parsing;

internal ref struct TemplateReader
{
	private readonly ReadOnlySpan<char> _source;

	public TemplateReader(ReadOnlySpan<char> source)
	{
		_source = source;
		Position = 0;
	}

	public int Position { get; private set; }

	public bool End => Position >= _source.Length;

	public char Current => _source[Position];

	public void Advance() => Position++;

	public bool TryConsume(char first, char second)
	{
		if (Position + 1 >= _source.Length ||
			_source[Position] != first ||
			_source[Position + 1] != second)
			return false;

		Position += 2;
		return true;
	}

	public ReadOnlySpan<char> ReadUntilBrace()
	{
		var start = Position;

		while (!End && Current is not ('{' or '}'))
			Advance();

		return _source.Slice(start, Position - start);
	}

	public ReadOnlySpan<char> Slice(int start, int end) => _source.Slice(start, end - start);
}
