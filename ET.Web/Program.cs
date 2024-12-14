using ET.Web.Data;
using ET.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add service for the DbContext, using In-memory database for now
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PGDBConnection"));
});

// Add service for Identity, using our ApplicatioUser
// model and our DbContext
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    // Set some loose-ish password policies for now
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add authentication and authorization services
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
});
builder.Services.AddAuthorization();

// Add health checks
builder.Services.AddHealthChecks()
                .AddDbContextCheck<ApplicationDbContext>(
                    name:"connectioncheck",
                    tags: ["ready"]
                )
;

// Add Data Protection, saved to DbContext
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>();

// Add Razor Pages

builder.Services.AddRazorPages();

var app = builder.Build();

// Test for invalid configuration
using (var scope = app.Services.CreateAsyncScope())
{
    ApplicationDbContext? db = scope.ServiceProvider.GetService<ApplicationDbContext>();
    ILogger logger = app.Logger;
    if (db == null)
    {
        logger.Log(LogLevel.Critical, "Could not connect to database service.");
        return 1;
    }

    try
    {
        // Try to perform migration at start
        db.Database.Migrate();

        // Check for presence of required tables
        var checkUsers = await db.Users.Where(u => 1 == 2).ToListAsync();
        var checkExpenses = await db.Expenses.Where(e => 1 == 2).ToListAsync();
    }
    catch(Exception ex)
    {
        logger.Log(LogLevel.Critical, "Database structure not valid: {error}", ex.ToString());
        return 1;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = healthCheck => healthCheck.Tags.Contains("ready")
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();

return 0;