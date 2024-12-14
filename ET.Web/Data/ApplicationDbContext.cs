using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ET.Web.Models;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

namespace ET.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
