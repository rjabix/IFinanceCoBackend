using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IFinanceCoBackend.Models;

public abstract class IFinanceDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
{
    public DbSet<Transaction> Transactions { get; set; }
}
public class FinanceDbContext(DbContextOptions<FinanceDbContext> options) : IFinanceDbContext(options)
{
    //public DbSet<Transaction> Transactions { get; set; } // - are in IFinanceDbContext
    
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     modelBuilder.Entity<AppUser>()
    //         .HasMany<Transaction>()
    //         .WithOne();
    // } Identity has no primary key?
}
