using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zagejmi.Application.Wallets;
using Zagejmi.Application.Wallets.ExpireBonuses;

namespace Zagejmi.Infrastructure.Wallets;

/// <summary>
///     Periodically returns expired bonus Goins to the promo budget (every <see cref="WalletOptions.BonusExpiryInterval" />).
/// </summary>
public sealed partial class BonusExpiryService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly TimeProvider timeProvider;
    private readonly WalletOptions options;
    private readonly ILogger<BonusExpiryService> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BonusExpiryService" /> class.
    /// </summary>
    /// <param name="scopeFactory">Creates a scope per run.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="options">The wallet rules.</param>
    /// <param name="logger">The logger.</param>
    public BonusExpiryService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<WalletOptions> options,
        ILogger<BonusExpiryService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.timeProvider = timeProvider;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(this.options.BonusExpiryInterval, this.timeProvider);

        do
        {
            try
            {
                await using AsyncServiceScope scope = this.scopeFactory.CreateAsyncScope();
                ExpireBonusesHandler handler = scope.ServiceProvider.GetRequiredService<ExpireBonusesHandler>();

                int expired = await handler.Handle(stoppingToken);
                if (expired > 0)
                {
                    this.LogExpired(expired);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // One failed run (e.g. the database restarting) must not stop all future runs.
                this.LogRunFailed(ex);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Expired {Count} bonus grants.")]
    private partial void LogExpired(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Expiring bonus grants failed.")]
    private partial void LogRunFailed(Exception exception);
}
