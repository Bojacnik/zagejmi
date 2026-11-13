using System;

namespace Zagejmi.Api.Filters;

/// <summary>
///     Marks an endpoint that changes state but must stay available in ghost mode (e.g. turning ghost mode off,
///     logging out). See <see cref="GhostModeFilter" />.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class AllowInGhostModeAttribute : Attribute
{
}
