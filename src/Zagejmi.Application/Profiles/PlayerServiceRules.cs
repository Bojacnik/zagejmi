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
    public const decimal MaxPrice = 1_000_000m;

    /// <summary>
    ///     Normalizes and validates the editable fields of a service.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="description">The description.</param>
    /// <param name="category">The category.</param>
    /// <param name="price">The price in Goins.</param>
    /// <param name="priceUnit">The price unit.</param>
    /// <returns>The trimmed values, or a validation failure.</returns>
    public static Result<ServiceFields> Validate(
        string name,
        string? description,
        string? category,
        decimal price,
        string? priceUnit)
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

        if (price is < 0 or > MaxPrice || decimal.Round(price, 2) != price)
        {
            return Fail("Service.Price", "Cena musí být mezi 0 a 1 000 000 Goinů (nejvýše na dvě desetinná místa).");
        }

        return new ServiceFields(trimmedName, trimmedDescription, trimmedCategory, price, trimmedPriceUnit);
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
/// <param name="Price">The price in Goins.</param>
/// <param name="PriceUnit">The price unit, or <see langword="null" />.</param>
internal sealed record ServiceFields(string Name, string Description, string? Category, decimal Price, string? PriceUnit);
