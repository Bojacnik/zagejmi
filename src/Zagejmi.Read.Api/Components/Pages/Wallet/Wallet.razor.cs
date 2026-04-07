using System;
using System.Collections.Generic;
using System.Linq;

using Zagejmi.Read.Api.Models;

namespace Zagejmi.Read.Api.Components.Pages.Wallet;

public partial class Wallet
{
    private readonly string[] _colors = ["accent", "sky", "emerald", "amber", "fuchsia"];

    public decimal TotalBalanceUsd { get; set; }

    public List<WalletViewModel> Wallets { get; set; } = [];

    public List<TransactionViewModel> Transactions { get; set; } = [];

    public List<ChartSegment> ChartSegments { get; set; } = [];

    protected override void OnInitialized()
    {
        this.InitializeData();
        this.CalculateChartSegments();
    }

    private void InitializeData()
    {
        Random random = new();
        string[] shuffledColors = this._colors.OrderBy(c => random.Next()).ToArray();

        this.Wallets =
        [
            new WalletViewModel
            {
                Id = Guid.NewGuid(), Name = "Primary Savings", Balance = 5248.75m, Currency = "USD",
                IconUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/40", BalanceInUsd = 5248.75m
            },
            new WalletViewModel
            {
                Id = Guid.NewGuid(), Name = "Ethereum", Balance = 12.5m, Currency = "ETH",
                IconUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/40", BalanceInUsd = 30123.45m
            },
            new WalletViewModel
            {
                Id = Guid.NewGuid(), Name = "Vacation Fund", Balance = 1234.56m, Currency = "EUR",
                IconUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/40", BalanceInUsd = 1350.25m
            },
            new WalletViewModel
            {
                Id = Guid.NewGuid(), Name = "Stock Portfolio", Balance = 8765.43m, Currency = "USD",
                IconUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/40", BalanceInUsd = 8765.43m
            },
            new WalletViewModel
            {
                Id = Guid.NewGuid(), Name = "Bitcoin", Balance = 0.5m, Currency = "BTC",
                IconUrl = $"https://picsum.photos/seed/{Guid.NewGuid()}/40", BalanceInUsd = 29876.54m
            }
        ];

        for (int i = 0; i < this.Wallets.Count; i++)
        {
            string color = shuffledColors[i % shuffledColors.Length];
            this.Wallets[i].ColorCssClass = $"border-2 border-{color}-500";
            this.Wallets[i].BackgroundColorCssClass = $"bg-{color}-500";
        }

        this.TotalBalanceUsd = this.Wallets.Sum(w => w.BalanceInUsd);

        Dictionary<string, Guid> walletIdLut = this.Wallets.ToDictionary(w => w.Name, w => w.Id);

        this.Transactions =
        [
            new TransactionViewModel
            {
                WalletId = walletIdLut["Primary Savings"],
                WalletName = "Primary Savings", Description = "Grocery Shopping", Date = DateTime.Now.AddDays(-1),
                Amount = -75.50m
            },
            new TransactionViewModel
            {
                WalletId = walletIdLut["Ethereum"],
                WalletName = "Ethereum", Description = "NFT Purchase", Date = DateTime.Now.AddDays(-2),
                Amount = -2.5m
            },
            new TransactionViewModel
            {
                WalletId = walletIdLut["Primary Savings"],
                WalletName = "Primary Savings", Description = "Salary Deposit", Date = DateTime.Now.AddDays(-3),
                Amount = 2500.00m
            },
            new TransactionViewModel
            {
                WalletId = walletIdLut["Vacation Fund"],
                WalletName = "Vacation Fund", Description = "Flight Tickets", Date = DateTime.Now.AddDays(-4),
                Amount = -450.00m
            },
            new TransactionViewModel
            {
                WalletId = walletIdLut["Stock Portfolio"],
                WalletName = "Stock Portfolio", Description = "Dividend", Date = DateTime.Now.AddDays(-5),
                Amount = 120.00m
            }
        ];
    }

    private void CalculateChartSegments()
    {
        if (this.TotalBalanceUsd == 0)
        {
            return;
        }

        double currentOffset = 0;
        foreach (WalletViewModel wallet in this.Wallets.OrderByDescending(w => w.BalanceInUsd))
        {
            double percentage = (double)(wallet.BalanceInUsd / this.TotalBalanceUsd) * 100;
            this.ChartSegments.Add(
                new ChartSegment
                {
                    Percentage = percentage,
                    Offset = -currentOffset,
                    StrokeColorCssClass = wallet.BackgroundColorCssClass.Replace("bg-", "stroke-")
                });
            currentOffset += percentage;
        }
    }
}