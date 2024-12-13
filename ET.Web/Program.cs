using ET.Web.Data;
using ET.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add service for the DbContext, using SQlit
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite("DataSource=appdata/et.db");
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
        var checkUsers = await db.Users.Where(u => 1 == 2).ToListAsync();
        var checkExpenses = await db.Expenses.Where(e => 1 == 2).ToListAsync();
    }
    catch
    {
        logger.Log(LogLevel.Critical, "Database structure not valid.");
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

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
return 0;