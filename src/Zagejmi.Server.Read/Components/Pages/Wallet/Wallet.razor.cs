using Zagejmi.Server.Read.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zagejmi.Server.Read.Components.Pages.Wallet
{
    public partial class Wallet
    {
        public decimal TotalBalanceUsd { get; set; }
        public List<WalletViewModel> Wallets { get; set; } = new();
        public List<TransactionViewModel> Transactions { get; set; } = new();
        public List<ChartSegment> ChartSegments { get; set; } = new();

        private readonly string[] _colors = { "accent", "sky", "emerald", "amber", "fuchsia" };

        protected override void OnInitialized()
        {
            InitializeData();
            CalculateChartSegments();
        }

        private void InitializeData()
        {
            Random random = new Random();
            var shuffledColors = _colors.OrderBy(c => random.Next()).ToArray();

            Wallets =
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

            for (int i = 0; i < Wallets.Count; i++)
            {
                string color = shuffledColors[i % shuffledColors.Length];
                Wallets[i].ColorCssClass = $"border-2 border-{color}-500";
                Wallets[i].BackgroundColorCssClass = $"bg-{color}-500";
            }

            TotalBalanceUsd = Wallets.Sum(w => w.BalanceInUsd);

            Transactions =
            [
                new TransactionViewModel
                {
                    WalletName = "Primary Savings", Description = "Grocery Shopping", Date = DateTime.Now.AddDays(-1),
                    Amount = -75.50m
                },
                new TransactionViewModel
                {
                    WalletName = "Ethereum", Description = "NFT Purchase", Date = DateTime.Now.AddDays(-2),
                    Amount = -2.5m
                },
                new TransactionViewModel
                {
                    WalletName = "Primary Savings", Description = "Salary Deposit", Date = DateTime.Now.AddDays(-3),
                    Amount = 2500.00m
                },
                new TransactionViewModel
                {
                    WalletName = "Vacation Fund", Description = "Flight Tickets", Date = DateTime.Now.AddDays(-4),
                    Amount = -450.00m
                },
                new TransactionViewModel
                {
                    WalletName = "Stock Portfolio", Description = "Dividend", Date = DateTime.Now.AddDays(-5),
                    Amount = 120.00m
                }
            ];
        }

        private void CalculateChartSegments()
        {
            if (TotalBalanceUsd == 0) return;

            double currentOffset = 0;
            foreach (WalletViewModel wallet in Wallets.OrderByDescending(w => w.BalanceInUsd))
            {
                double percentage = (double)(wallet.BalanceInUsd / TotalBalanceUsd) * 100;
                ChartSegments.Add(new ChartSegment
                {
                    Percentage = percentage,
                    Offset = -currentOffset,
                    StrokeColorCssClass = wallet.BackgroundColorCssClass.Replace("bg-", "stroke-")
                });
                currentOffset += percentage;
            }
        }
    }
}
