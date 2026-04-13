using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Components.Pages.Wallet;

/// <summary>
///     Displays details and transaction history for a selected wallet.
/// </summary>
public partial class WalletDetail : ComponentBase
{
    /// <summary>
    ///     Gets or sets the wallet identifier to display.
    /// </summary>
    [Parameter]
    public Guid WalletId { get; set; }

    /// <summary>
    ///     Gets or sets the loaded wallet details.
    /// </summary>
    public WalletDetailViewModel? Wallet { get; set; }

    /// <summary>
    ///     Loads wallet details for the selected wallet.
    /// </summary>
    /// <returns>A task that completes when the wallet details are initialized.</returns>
    protected override async Task OnInitializedAsync()
    {
        // In a real application, you would fetch the wallet data from a service.
        // For now, we'll create some sample data.
        await Task.Delay(500); // Simulate network latency

        this.Wallet = new WalletDetailViewModel
        {
            Id = this.WalletId,
            Name = "My Main Wallet",
            Balance = 1234.56m,
            Currency = "USD",
            IconUrl = "/img/wallet-icons/dollar.svg",
            ColorCssClass = "stroke-accent-500",
            BackgroundColorCssClass = "bg-accent-500",
            BalanceInUsd = 1234.56m,
            Transactions = new List<TransactionViewModel>
            {
                new()
                {
                    WalletName = "My Main Wallet",
                    Description = "Salary",
                    Date = new DateTime(2023, 10, 26),
                    Amount = 2000.00m
                },
                new()
                {
                    WalletName = "My Main Wallet",
                    Description = "Groceries",
                    Date = new DateTime(2023, 10, 25),
                    Amount = -75.50m
                },
                new()
                {
                    WalletName = "My Main Wallet",
                    Description = "Gas",
                    Date = new DateTime(2023, 10, 24),
                    Amount = -40.00m
                }
            }
        };
    }
}