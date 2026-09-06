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
}