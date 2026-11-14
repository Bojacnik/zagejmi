using System;

using Microsoft.EntityFrameworkCore;

using Zagejmi.Domain.Orders;
using Zagejmi.Domain.Profiles.Players;
using Zagejmi.Domain.Wallets;
using Zagejmi.Infrastructure.Wallets.Records;

namespace Zagejmi.Infrastructure.Wallets.Ctx;

/// <summary>
///     The DbContext of the Goin ledger (schema <c>wallets</c>). Orders will live here too, so changing an order and
///     moving its Goins can happen in one database transaction.
/// </summary>
public class WalletsContext : DbContext
{
    private static readonly DateTimeOffset SystemAccountsCreatedAt = new(2026, 11, 13, 0, 0, 0, TimeSpan.Zero);

    /// <summary>
    ///     Initializes a new instance of the <see cref="WalletsContext" /> class.
    /// </summary>
    /// <param name="options">The context options.</param>
    public WalletsContext(DbContextOptions<WalletsContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Gets or sets the ledger accounts.
    /// </summary>
    public DbSet<AccountRecord> Accounts { get; set; }

    /// <summary>
    ///     Gets or sets the ledger transactions.
    /// </summary>
    public DbSet<LedgerTransactionRecord> Transactions { get; set; }

    /// <summary>
    ///     Gets or sets the ledger entries.
    /// </summary>
    public DbSet<LedgerEntryRecord> Entries { get; set; }

    /// <summary>
    ///     Gets or sets the bonus grants.
    /// </summary>
    public DbSet<BonusGrantRecord> BonusGrants { get; set; }

    /// <summary>
    ///     Gets or sets the orders (their money moves through the ledger in the same transaction).
    /// </summary>
    public DbSet<OrderRecord> Orders { get; set; }

    /// <summary>
    ///     Configures the model.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("wallets");

        modelBuilder.Entity<AccountRecord>(entity =>
        {
            entity.ToTable("Accounts", table => table.HasCheckConstraint(
                "CK_Accounts_NotNegative",
                "\"AllowNegative\" OR \"Balance\" >= 0"));

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Kind).HasConversion<string>().HasMaxLength(30);

            // One account of each kind per profile. System accounts have no owner (NULLs are distinct).
            entity.HasIndex(e => new { e.OwnerProfileId, e.Kind }).IsUnique();

            entity.HasData(
                SystemAccount(AccountKind.PromoBudget),
                SystemAccount(AccountKind.PlatformRevenue),
                SystemAccount(AccountKind.Escrow),
                SystemAccount(AccountKind.TopUpClearing),
                SystemAccount(AccountKind.PayoutClearing));
        });

        modelBuilder.Entity<LedgerTransactionRecord>(entity =>
        {
            entity.ToTable("LedgerTransactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Type).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Description).HasMaxLength(LedgerTransaction.DescriptionMaxLength);
            entity.Property(e => e.IdempotencyKey).HasMaxLength(LedgerTransaction.IdempotencyKeyMaxLength);

            // Posting the same operation twice (double click, retry) is rejected here.
            entity.HasIndex(e => new { e.Type, e.IdempotencyKey }).IsUnique();
            entity.HasIndex(e => e.CreatedAt);

            entity.HasMany(e => e.Entries)
                .WithOne()
                .HasForeignKey(e => e.TransactionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LedgerEntryRecord>(entity =>
        {
            entity.ToTable("LedgerEntries", table =>
            {
                table.HasCheckConstraint("CK_LedgerEntries_NotZero", "\"Amount\" <> 0");
                table.HasCheckConstraint(
                    "CK_LedgerEntries_BonusHasGrant",
                    "(\"Funding\" = 'Bonus') = (\"BonusGrantId\" IS NOT NULL)");
            });

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.AccountKind).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Funding).HasConversion<string>().HasMaxLength(10);

            entity.HasOne<AccountRecord>()
                .WithMany()
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<BonusGrantRecord>()
                .WithMany()
                .HasForeignKey(e => e.BonusGrantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.AccountId, e.TransactionId });
        });

        modelBuilder.Entity<BonusGrantRecord>(entity =>
        {
            entity.ToTable("BonusGrants", table => table.HasCheckConstraint(
                "CK_BonusGrants_Remaining",
                "\"Remaining\" >= 0 AND \"Remaining\" <= \"Amount\""));

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Reason).HasMaxLength(BonusGrant.ReasonMaxLength);

            entity.HasIndex(e => e.CustomerProfileId);

            // The expiry job only looks at grants with something left.
            entity.HasIndex(e => e.ExpiresAt).HasFilter("\"Remaining\" > 0");
        });

        modelBuilder.Entity<OrderRecord>(entity =>
        {
            entity.ToTable("Orders", table =>
            {
                table.HasCheckConstraint("CK_Orders_Quantity", $"\"Quantity\" BETWEEN 1 AND {Order.MaxQuantity}");
                table.HasCheckConstraint("CK_Orders_Amounts", "\"Total\" >= 0 AND \"Fee\" >= 0 AND \"Fee\" <= \"Total\"");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ServiceName).HasMaxLength(PlayerService.NameMaxLength);
            entity.Property(e => e.PriceUnit).HasMaxLength(PlayerService.PriceUnitMaxLength);
            entity.Property(e => e.DisputeReason).HasMaxLength(Order.DisputeReasonMaxLength);
            entity.Property(e => e.Outcome).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Cancellation).HasConversion<string>().HasMaxLength(20);

            // A change is only saved if the order is still in the state it was read in, so two people acting at once
            // (accept vs. cancel) can never both win.
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20).IsConcurrencyToken();

            entity.HasIndex(e => new { e.CustomerProfileId, e.CreatedAt });
            entity.HasIndex(e => new { e.PlayerProfileId, e.CreatedAt });

            // The timeout job looks for pending and finished orders past their deadline.
            entity.HasIndex(e => e.AcceptDeadline).HasFilter("\"Status\" = 'Pending'");
            entity.HasIndex(e => e.ConfirmDeadline).HasFilter("\"Status\" = 'Finished'");
            entity.HasIndex(e => e.DisputedAt).HasFilter("\"Status\" = 'Disputed'");
        });

        base.OnModelCreating(modelBuilder);
    }

    private static AccountRecord SystemAccount(AccountKind kind)
    {
        return new AccountRecord
        {
            Id = SystemAccounts.IdOf(kind),
            Kind = kind,
            OwnerProfileId = null,
            Balance = 0,

            // Only the accounts that stand for money leaving the platform's books can go negative. Escrow, revenue and
            // payouts going negative would mean a bug, so the check constraint guards them like user accounts.
            AllowNegative = kind is AccountKind.PromoBudget or AccountKind.TopUpClearing,
            CreatedAt = SystemAccountsCreatedAt,
        };
    }
}
