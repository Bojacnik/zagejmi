using Zagejmi.Domain.Games;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Shared;

namespace Zagejmi.Application.Profiles;

/// <summary>
///     Validation shared by adding and editing a player service.
/// </summary>
internal static class PlayerServiceRules
{
    /// <summary>
    ///     The most services one player can offer.
    /// </summary>
    public const int MaxServicesPerPlayer = 50;

    /// <summary>
    ///     The highest allowed price in Goins.
    /// </summary>
    public const long MaxPrice = 1_000_000;

    /// <summary>
    ///     Normalizes and validates the editable fields of a service.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    /// <param name="price">The price in Goins.</param>
    /// <param name="priceUnit">The price unit.</param>
    /// <param name="gameKey">The game (a <see cref="GameCatalog" /> key), or <see langword="null" /> for no game.</param>
    /// <param name="rankKey">The player's rank in the game, or <see langword="null" />.</param>
    /// <param name="playStyles">Competitive, casual or both; required when a game is set.</param>
    /// <returns>The trimmed values, or a validation failure.</returns>
    public static Result<ServiceFields> Validate(
        string name,
        string? description,
        string? category,
        long price,
        string? priceUnit,
        string? gameKey,
        string? rankKey,
        PlayStyles playStyles)
    {
        string trimmedName = name.Trim();
        string trimmedDescription = description?.Trim() ?? string.Empty;
        string? trimmedCategory = NullIfEmpty(category);
        string? trimmedPriceUnit = NullIfEmpty(priceUnit);

        if (trimmedName.Length is 0 or > PlayerService.NameMaxLength)
        {
            return Fail("Service.Name", $"Název služby musí mít 1 až {PlayerService.NameMaxLength} znaků.");
        }

        if (trimmedDescription.Length > PlayerService.DescriptionMaxLength)
        {
            return Fail("Service.Description", $"Popis může mít nejvýše {PlayerService.DescriptionMaxLength} znaků.");
        }

        if (trimmedCategory?.Length > PlayerService.CategoryMaxLength)
        {
            return Fail("Service.Category", $"Kategorie může mít nejvýše {PlayerService.CategoryMaxLength} znaků.");
        }

        if (trimmedPriceUnit?.Length > PlayerService.PriceUnitMaxLength)
        {
            return Fail("Service.PriceUnit", $"Jednotka ceny může mít nejvýše {PlayerService.PriceUnitMaxLength} znaků.");
        }

        if (price is < 0 or > MaxPrice)
        {
            return Fail("Service.Price", "Cena musí být celé číslo od 0 do 1 000 000 Goinů.");
        }

        Result<GameFields> game = ValidateGame(NullIfEmpty(gameKey), NullIfEmpty(rankKey), playStyles);
        if (game.IsFailure)
        {
            return Result<ServiceFields>.Failure(game.Error!);
        }

        return new ServiceFields(trimmedName, trimmedDescription, trimmedCategory, price, trimmedPriceUnit, game.Value);
    }

    /// <summary>
    ///     Checks that the game and rank come from the catalog and that the play style fits.
    /// </summary>
    private static Result<GameFields> ValidateGame(string? gameKey, string? rankKey, PlayStyles playStyles)
    {
        if (gameKey is null)
        {
            // Without a game there is nothing to rank or to play competitively; ignore leftovers from the form.
            return new GameFields(null, null, PlayStyles.None);
        }

        Game? game = GameCatalog.Find(gameKey);
        if (game is null)
        {
            return GameFail("Service.Game", "Vyberte hru ze seznamu.");
        }

        if (rankKey is not null && game.FindRank(rankKey) is null)
        {
            return GameFail("Service.Rank", $"Vyberte rank ze seznamu pro {game.Name}.");
        }

        if ((playStyles & ~PlayStyles.Both) != 0 || playStyles == PlayStyles.None)
        {
            return GameFail("Service.PlayStyles", "Vyberte, jestli hrajete kompetitivně, nekompetitivně, nebo obojí.");
        }

        return new GameFields(game.Key, rankKey, playStyles);
    }

    private static Result<GameFields> GameFail(string code, string message)
    {
        return Result<GameFields>.Failure(ErrorDetail.Validation(code, message));
    }

    private static Result<ServiceFields> Fail(string code, string message)
    {
        return Result<ServiceFields>.Failure(ErrorDetail.Validation(code, message));
    }

    private static string? NullIfEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

/// <summary>
///     Validated, trimmed service fields.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Description">The description.</param>
/// <param name="Category">The category, or <see langword="null" />.</param>
/// <param name="Price">The price in whole Goins.</param>
/// <param name="PriceUnit">The price unit, or <see langword="null" />.</param>
/// <param name="Game">The validated game, rank and play style.</param>
internal sealed record ServiceFields(
    string Name,
    string Description,
    string? Category,
    long Price,
    string? PriceUnit,
    GameFields Game);

/// <summary>
///     Validated game fields of a service.
/// </summary>
/// <param name="GameKey">The game key, or <see langword="null" /> when the service is not tied to a game.</param>
/// <param name="RankKey">The rank key, or <see langword="null" />.</param>
/// <param name="PlayStyles">The play style; <see cref="PlayStyles.None" /> exactly when there is no game.</param>
internal sealed record GameFields(string? GameKey, string? RankKey, PlayStyles PlayStyles);
