namespace EggERP.Domain.Entities;

public class Business
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? LegalName { get; set; }

    public string? TaxIdentificationNumber { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? Province { get; set; }

    public string? PostalCode { get; set; }

    public string CountryCode { get; set; } = "PH";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}