using System;
using System.Reflection;

using Zagejmi.Write.Domain.Abstractions;

namespace Zagejmi.Write.Infrastructure.EventStore;

internal static class EventTypeNameResolver
{
    private const string DomainNamespacePrefix = "Zagejmi.Write.Domain.";
    private static readonly Assembly DomainAssembly = typeof(IDomainEvent).Assembly;

    public static string ToStoredName(Type eventType)
    {
        string fullName = eventType.FullName ?? eventType.Name;

        return fullName.StartsWith(DomainNamespacePrefix, StringComparison.Ordinal)
            ? fullName[DomainNamespacePrefix.Length..]
            : fullName;
    }

    public static Type? Resolve(string storedEventType)
    {
        if (string.IsNullOrWhiteSpace(storedEventType))
        {
            return null;
        }

        // Backward compatibility: resolve existing assembly-qualified values first.
        Type? resolvedType = Type.GetType(storedEventType, false);
        if (resolvedType != null)
        {
            return resolvedType;
        }

        string normalizedTypeName = storedEventType;
        int commaIndex = normalizedTypeName.IndexOf(',');
        if (commaIndex >= 0)
        {
            normalizedTypeName = normalizedTypeName[..commaIndex].Trim();
        }

        if (normalizedTypeName.StartsWith(DomainNamespacePrefix, StringComparison.Ordinal))
        {
            return DomainAssembly.GetType(normalizedTypeName, false);
        }

        return DomainAssembly.GetType($"{DomainNamespacePrefix}{normalizedTypeName}", false);
    }
}