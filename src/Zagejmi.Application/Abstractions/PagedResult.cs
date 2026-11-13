using System.Collections.Generic;

namespace Zagejmi.Application.Abstractions;

/// <summary>
///     One page of a list together with the number of all matching items.
/// </summary>
/// <typeparam name="T">The item.</typeparam>
/// <param name="Items">The items on the page.</param>
/// <param name="TotalCount">How many items match in total (all pages).</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
