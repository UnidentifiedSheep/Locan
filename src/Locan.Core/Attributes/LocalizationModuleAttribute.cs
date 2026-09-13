using System;

namespace Locan.Core.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class LocalizationModuleAttribute : Attribute
{
	public LocalizationModuleAttribute(string name)
	{
		Name = name;
	}

	public string Name { get; }

	public bool IsInternal { get; set; }
}
