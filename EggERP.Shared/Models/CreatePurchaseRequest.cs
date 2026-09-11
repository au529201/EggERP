namespace EggERP.Shared.Models
{
    public class CreatePurchaseItemInput
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
    public class CreatePurchaseRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? SupplierId { get; set; }
        public List<CreatePurchaseItemInput> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = "Cash";
        public string? PaymentSource { get; set; }
        public string? ReferenceNumber { get; set; }
    }
}