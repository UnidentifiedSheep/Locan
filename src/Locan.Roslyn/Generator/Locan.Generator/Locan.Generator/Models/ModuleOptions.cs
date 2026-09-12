namespace Locan.Generator.Models;

internal sealed class ModuleOptions
{
	public ModuleOptions(string name, bool isInternal)
	{
		Name = name;
		IsInternal = isInternal;
	}

	public string Name { get; }

	public bool IsInternal { get; }
}
