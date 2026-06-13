using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Models.Domain;

namespace PromptMarketPlace.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<CreatorProfile> CreatorProfiles => Set<CreatorProfile>();
    public DbSet<UserWallet> Wallets => Set<UserWallet>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<AiProvider> AiProviders => Set<AiProvider>();
    public DbSet<AiModel> AiModels => Set<AiModel>();
    public DbSet<AppCategory> Categories => Set<AppCategory>();
    public DbSet<AiApp> Apps => Set<AiApp>();
    public DbSet<AppInputField> AppInputFields => Set<AppInputField>();
    public DbSet<AppTag> AppTags => Set<AppTag>();
    public DbSet<AppExecution> Executions => Set<AppExecution>();
    public DbSet<ExecutionInputValue> ExecutionInputValues => Set<ExecutionInputValue>();
    public DbSet<AppShowcaseItem> ShowcaseItems => Set<AppShowcaseItem>();
    public DbSet<AppReview> Reviews => Set<AppReview>();
    public DbSet<CreditPackage> CreditPackages => Set<CreditPackage>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<UserFavorite> Favorites => Set<UserFavorite>();
    public DbSet<AdminAuditLog> AuditLogs => Set<AdminAuditLog>();
    public DbSet<MessageThread> MessageThreads => Set<MessageThread>();
    public DbSet<ThreadMessage> ThreadMessages => Set<ThreadMessage>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
