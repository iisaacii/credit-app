using CreditApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<CreditRequest> CreditRequests => Set<CreditRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(e =>
        {
            e.HasKey(x => x.ClientId);
            e.Property(x => x.FirstName).IsRequired();
            e.Property(x => x.LastName).IsRequired();
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.HasKey(x => x.BranchId);
            e.Property(x => x.Name).IsRequired();
        });

        modelBuilder.Entity<CreditRequest>(e =>
        {
            e.HasKey(x => x.CreditRequestId);
            e.HasOne(x => x.Client)
             .WithMany(c => c.CreditRequests)
             .HasForeignKey(x => x.ClientId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Branch)
             .WithMany()
             .HasForeignKey(x => x.BranchId)
             .OnDelete(DeleteBehavior.NoAction);
            e.Property(x => x.Status).HasMaxLength(20);
        });
    }
}
