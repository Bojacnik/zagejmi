using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Zagejmi.Application.Orders;
using Zagejmi.Application.Orders.ProcessOrderTimeouts;

namespace Zagejmi.Infrastructure.Wallets;

/// <summary>
///     Periodically cancels orders nobody accepted in time and completes finished orders nobody confirmed in time.
/// </summary>
public sealed partial class OrderTimeoutService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly TimeProvider timeProvider;
    private readonly OrderOptions options;
    private readonly ILogger<OrderTimeoutService> logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrderTimeoutService" /> class.
    /// </summary>
    /// <param name="scopeFactory">Creates a scope per run.</param>
    /// <param name="timeProvider">The clock.</param>
    /// <param name="options">The order settings.</param>
    /// <param name="logger">The logger.</param>
    public OrderTimeoutService(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        IOptions<OrderOptions> options,
        ILogger<OrderTimeoutService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.timeProvider = timeProvider;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(this.options.TimeoutInterval, this.timeProvider);
        do
        {
            try
            {
                await using AsyncServiceScope scope = this.scopeFactory.CreateAsyncScope();
                ProcessOrderTimeoutsHandler handler = scope.ServiceProvider.GetRequiredService<ProcessOrderTimeoutsHandler>();
                int changed = await handler.Handle(stoppingToken);
                if (changed > 0)
                {
                    this.LogProcessed(changed);
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Timed out {Count} orders.")]
    private partial void LogProcessed(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Processing order timeouts failed.")]
    private partial void LogRunFailed(Exception exception);
}
