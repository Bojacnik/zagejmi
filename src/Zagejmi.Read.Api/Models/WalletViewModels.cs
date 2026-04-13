using System;
using System.Collections.Generic;

namespace Zagejmi.Read.Api.Models;

/// <summary>
///     Summary view model for a wallet card.
/// </summary>
public class WalletViewModel
{
    /// <summary>
    ///     Gets or sets the wallet identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the wallet display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the balance in wallet currency.
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    ///     Gets or sets the wallet currency code.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the icon URL.
    /// </summary>
    public string IconUrl { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets CSS class for accent color.
    /// </summary>
    public string ColorCssClass { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets CSS class for background color.
    /// </summary>
    public string BackgroundColorCssClass { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the balance converted to USD.
    /// </summary>
    public decimal BalanceInUsd { get; set; }
}

/// <summary>
///     Detailed wallet view model including transactions.
/// </summary>
public class WalletDetailViewModel : WalletViewModel
{
    /// <summary>
    ///     Gets or sets transactions associated with the wallet.
    /// </summary>
    public List<TransactionViewModel> Transactions { get; set; } = new();
}

/// <summary>
///     View model for a wallet transaction.
/// </summary>
public class TransactionViewModel
{
    /// <summary>
    ///     Gets or sets the related wallet identifier.
    /// </summary>
    public Guid WalletId { get; set; }

    /// <summary>
    ///     Gets or sets the wallet name.
    /// </summary>
    public string WalletName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the transaction description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets transaction date.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    ///     Gets or sets transaction amount.
    /// </summary>
    public decimal Amount { get; set; }
}

/// <summary>
///     Segment metadata for wallet composition charts.
/// </summary>
public class ChartSegment
{
    /// <summary>
    ///     Gets or sets CSS class for chart stroke color.
    /// </summary>
    public string StrokeColorCssClass { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets segment percentage.
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    ///     Gets or sets offset value used for rendering.
    /// </summary>
    public double Offset { get; set; }
}