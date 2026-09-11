using System;

namespace Locan.Generator.Attributes;

/// <summary>
/// Specifies assembly that is used for localization source generation
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class LocalizationModuleAttribute : Attribute
{
    /// <param name="name">The module name</param>
    public LocalizationModuleAttribute(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets module name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies if the generated module shall be internal.
    /// </summary>
    public bool IsInternal { get; set; }
}
