using System.Reflection;

namespace Zagejmi.Shared.Util;

/// <summary>
///     A simple mapper implementation that maps properties from one object to another based on matching property names and
///     types.
/// </summary>
public class SimpleMapper : IMapper
{
    /// <summary>
    ///     Maps properties from the source object to a new instance of the target type. Only properties with matching names
    ///     and types will be mapped.
    /// </summary>
    /// <param name="from">The source object to map from.</param>
    /// <typeparam name="TFrom">The type of the source object.</typeparam>
    /// <typeparam name="TTo">The type of the target object to map to.</typeparam>
    /// <returns>A new instance of the target type with mapped properties, or null if the source object is null.</returns>
    public TTo? Map<TFrom, TTo>(TFrom from)
    {
        if (from == null)
        {
            return default;
        }

        TTo to = Activator.CreateInstance<TTo>();
        PropertyInfo[] fromProperties = typeof(TFrom).GetProperties();
        PropertyInfo[] toProperties = typeof(TTo).GetProperties();

        foreach (PropertyInfo fromProperty in fromProperties)
        {
            PropertyInfo? toProperty = toProperties.FirstOrDefault(p =>
                p.Name == fromProperty.Name && p.PropertyType == fromProperty.PropertyType);
            if (toProperty == null || !toProperty.CanWrite)
            {
                continue;
            }

            object? value = fromProperty.GetValue(from);
            toProperty.SetValue(to, value);
        }

        return to;
    }
}