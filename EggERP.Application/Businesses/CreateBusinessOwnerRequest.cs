using System.ComponentModel.DataAnnotations;

namespace EggERP.Application.Businesses;

public class CreateBusinessOwnerRequest
{
    [Required]
    [MaxLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LegalName { get; set; }

    [EmailAddress]
    [MaxLength(256)]
    public string? BusinessEmail { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? AddressLine1 { get; set; }

    [MaxLength(200)]
    public string? AddressLine2 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(20)]
    public string? PostalCode { get; set; }

    [Required]
    [MaxLength(100)]
    public string OwnerFullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string OwnerEmail { get; set; } = string.Empty;
}