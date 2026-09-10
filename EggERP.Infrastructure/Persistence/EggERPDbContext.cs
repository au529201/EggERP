using EggERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Persistence;

public class EggERPDbContext : DbContext
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
    public DbSet<EggERP.Domain.Entities.Inventory> Inventories => Set<EggERP.Domain.Entities.Inventory>(); public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
} 