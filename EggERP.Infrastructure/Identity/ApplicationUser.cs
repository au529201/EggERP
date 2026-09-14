using Microsoft.AspNetCore.Identity;

namespace EggERP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // Each user belongs to exactly one business.
    // Intentionally a plain scalar property, no navigation to Business,
    // so EF does not generate a foreign key against the Businesses table.
    public Guid BusinessId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}