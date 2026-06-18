using Microsoft.EntityFrameworkCore;
using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.ReferenceNumber).IsUnique();
            entity.Property(t => t.ReferenceNumber).IsRequired();
            entity.Property(t => t.Amount).HasPrecision(18, 2);
        });
    }
}
