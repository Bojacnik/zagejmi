using System.Reflection;

namespace Zagejmi.SharedKernel.Util;

public class Mapper : IMapper
{
    public TTo? Map<TFrom, TTo>(TFrom from)
    {
        if (from == null)
        {
            return default(TTo);
        }

        TTo? to = Activator.CreateInstance<TTo>();
        PropertyInfo[] fromProperties = typeof(TFrom).GetProperties();
        PropertyInfo[] toProperties = typeof(TTo).GetProperties();

        foreach (PropertyInfo fromProperty in fromProperties)
        {
            PropertyInfo? toProperty = toProperties.FirstOrDefault(p => p.Name == fromProperty.Name && p.PropertyType == fromProperty.PropertyType);
            if (toProperty == null || !toProperty.CanWrite) continue;
            
            object? value = fromProperty.GetValue(from);
            toProperty.SetValue(to, value);
        }

        return to;
    }
}