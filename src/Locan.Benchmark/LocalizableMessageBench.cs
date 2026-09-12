using BenchmarkDotNet.Attributes;
using Locan.Core.Interfaces;
using Locan.Core.LocalizableMessages;

namespace Locan.Benchmark;

[MemoryDiagnoser]
public class LocalizableMessageBench
{
	[Benchmark]
	public ILocalizableMessage LocalizableMessage()
	{
		return new LocalizableMessage("article.not.found", 4)
			.WithValue("SomeKey", 123)
			.WithValue("SomeKey1", "str")
			.WithValue("SomeDecimal", 123.22m)
			.WithValue("SomeKeyByte", byte.MaxValue);
	}

	[Benchmark]
	public ILocalizableMessage Empty() => new LocalizableMessage("article.not.found");

	[Benchmark]
	public ILocalizableMessage StringsOnly()
	{
		return new LocalizableMessage("article.not.found", 4)
			.WithValue("SomeKey", "123")
			.WithValue("SomeKey1", "str")
			.WithValue("SomeDecimal", "123.22")
			.WithValue("SomeKeyByte", "255");
	}

	[Benchmark]
	public ILocalizableMessage OneInt()
	{
		return new LocalizableMessage("article.not.found", 1)
			.WithValue("SomeKey", 123);
	}

	[Benchmark]
	public ILocalizableMessage OneString()
	{
		return new LocalizableMessage("article.not.found", 1)
			.WithValue("SomeKey", "123");
	}

	[Benchmark]
	public ILocalizableMessage OneDecimal()
	{
		return new LocalizableMessage("article.not.found", 1)
			.WithValue("SomeKey", 123.22m);
	}
}
