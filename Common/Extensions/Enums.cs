using System;
using Common;
using Common.Attributes;
using System.Reflection;

/// <summary>
/// Provides functionality to enhance enumerations.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Returns the description of the specified enum.
    /// </summary>
    /// <param name="value">The value of the enum for which to return the description.</param>
    /// <returns>A description of the enum, or the enum name if no description exists.</returns>
    public static string GetDescription(this Enum value)
    {
        return GlobalUtilities.GetEnumDescription(value);
    }

    public static string GetValueDescription(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());

        DescriptionAttribute attribute
                = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                    as DescriptionAttribute;

        return attribute == null ? value.ToString() : attribute.Description;
    }
}