using System.Reflection;

namespace Zagejmi.Shared;

/// <summary>
///     Static class for resolving event names to types.
/// </summary>
public static class EventTypeNameResolver
{
    /// <summary>
    ///     Resolves type from string.
    /// </summary>
    /// <param name="typeName">Name of the type.</param>
    /// <returns>Optional type.</returns>
    public static Type? Resolve(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        // Try to find by full name or by short name across loaded assemblies
        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type? type = asm.GetType(typeName, false, true);
                if (type != null)
                {
                    return type;
                }

                type = asm.GetTypes().FirstOrDefault(t => string.Equals(
                    t.Name,
                    typeName,
                    StringComparison.OrdinalIgnoreCase));

                if (type != null)
                {
                    return type;
                }
            }
            catch
            {
                // ignore assemblies we can't scan
            }
        }

        return null;
    }
}