using EggERP.Domain.Entities;
using EggERP.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Persistence;

public class EggERPDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public EggERPDbContext(DbContextOptions<EggERPDbContext> options)
        : base(options)
    {
    }

    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<EggERP.Domain.Entities.Inventory> Inventories => Set<EggERP.Domain.Entities.Inventory>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<Flock> Flocks => Set<Flock>();
    public DbSet<FlockMovement> FlockMovements => Set<FlockMovement>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Base call configures all Identity table mappings (AspNetUsers, AspNetRoles, etc.).
        // No overrides needed for existing ERP entities; their configuration is unchanged.
    }
}