namespace Zagejmi.Server.Read.Models;

public class WalletViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string ColorCssClass { get; set; } = string.Empty;
    public string BackgroundColorCssClass { get; set; } = string.Empty;
    public decimal BalanceInUsd { get; set; }
}

public class TransactionViewModel
{
    public string WalletName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class ChartSegment
{
    public string StrokeColorCssClass { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public double Offset { get; set; }
}