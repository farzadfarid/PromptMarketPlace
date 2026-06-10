using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PromptMarketPlace.Data;
using PromptMarketPlace.Models.Domain;
using PromptMarketPlace.Models.Enums;
using PromptMarketPlace.Services;
using PromptMarketPlace.Services.Interfaces;
using Serilog;
using Serilog.Events;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        configuration["Serilog:FilePath"] ?? "Logs/log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting PromptMarketPlace...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.SignIn.RequireConfirmedEmail = true;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("CreatorOnly", policy => policy.RequireRole("Creator", "Admin"));
        options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    });

    builder.Services.AddHttpClient("OpenRouter");

    builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
    builder.Services.AddScoped<IAiProviderService, AiProviderService>();
    builder.Services.AddScoped<ICreditService, CreditService>();
    builder.Services.AddScoped<IAiService, AiService>();
    builder.Services.AddScoped<IStorageService, LocalStorageService>();
    builder.Services.AddScoped<IExecutionService, ExecutionService>();
    builder.Services.AddScoped<ISettingService, SettingService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();
    builder.Services.AddScoped<ISlugService, SlugService>();
    builder.Services.AddScoped<IAppService, AppService>();
    builder.Services.AddScoped<IReviewService, ReviewService>();
    builder.Services.AddScoped<IWithdrawalService, WithdrawalService>();
    builder.Services.AddScoped<ICreatorHelper, CreatorHelper>();
    builder.Services.AddScoped<IMessageService, MessageService>();
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
    builder.Services.AddMemoryCache();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(10);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });
    builder.Services.AddSingleton<ICaptchaService, CaptchaService>();

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeAreaFolder("Admin", "/", "AdminOnly");
        options.Conventions.AuthorizeAreaFolder("Creator", "/", "CreatorOnly");
        options.Conventions.AuthorizeAreaFolder("User", "/", "AuthenticatedUser");
    });

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    // ─── Seed Roles + Admin User ──────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { "Admin", "Creator", "User" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        async Task SeedUser(string email, string password, string displayName,
            UserRole role, string identityRole, bool withCreatorProfile = false)
        {
            if (await userManager.FindByEmailAsync(email) != null) return;
            var user = new ApplicationUser
            {
                UserName = email, Email = email, DisplayName = displayName,
                EmailConfirmed = true, Role = role, IsActive = true
            };
            var r = await userManager.CreateAsync(user, password);
            if (!r.Succeeded) return;
            await userManager.AddToRoleAsync(user, "User");
            if (identityRole != "User") await userManager.AddToRoleAsync(user, identityRole);
            db.Wallets.Add(new UserWallet { UserId = user.Id, CreditBalance = 100 });
            if (withCreatorProfile)
                db.CreatorProfiles.Add(new CreatorProfile { UserId = user.Id, CommissionPercent = 70 });
            await db.SaveChangesAsync();
        }

        await SeedUser("admin@promptmarket.ir",   "Admin@1234",   "ادمین سیستم", UserRole.Admin,   "Admin");
        await SeedUser("creator@promptmarket.ir", "Creator@1234", "علی سازنده",  UserRole.Creator, "Creator", withCreatorProfile: true);
        await SeedUser("user@promptmarket.ir",    "User@1234",    "سارا کاربر",  UserRole.User,    "User");
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.UseRouting();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapStaticAssets();
    app.MapRazorPages().WithStaticAssets();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
