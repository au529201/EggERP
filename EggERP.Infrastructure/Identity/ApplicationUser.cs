using Microsoft.AspNetCore.Identity;

namespace EggERP.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    // Nullable: SuperAdmin accounts are not tied to a single business.
    // Intentionally a plain scalar property, no navigation to Business,
    // so EF does not generate a foreign key aginst the Businesses table.
    public Guid? BusinessId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}