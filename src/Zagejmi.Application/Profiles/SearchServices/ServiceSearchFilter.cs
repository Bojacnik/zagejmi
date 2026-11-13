using System.Collections.Generic;

using Zagejmi.Domain.Games;

namespace Zagejmi.Application.Profiles.SearchServices;

/// <summary>
///     A validated service search, ready for the repository.
/// </summary>
/// <param name="Search">Trimmed text to look for in the service name or player name, or <see langword="null" />.</param>
/// <param name="GameKey">The game key, or <see langword="null" /> for any game.</param>
/// <param name="RankKeys">The rank keys that match, or <see langword="null" /> for any rank (including none).</param>
/// <param name="PlayStyles">Match services offering any of these styles; <see cref="PlayStyles.None" /> for any.</param>
/// <param name="Gender">Only services of players in this gender group, or <see langword="null" /> for everyone.</param>
public sealed record ServiceSearchFilter(
    string? Search,
    string? GameKey,
    IReadOnlyCollection<string>? RankKeys,
    PlayStyles PlayStyles,
    GenderGroup? Gender = null);
