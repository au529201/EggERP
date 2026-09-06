using Microsoft.EntityFrameworkCore;

namespace EggERP.Infrastructure.Persistence;

public class EggERPDbContext : DbContext
{
    public EggERPDbContext(DbContextOptions<EggERPDbContext> options)
        : base(options)
    {
    }
}