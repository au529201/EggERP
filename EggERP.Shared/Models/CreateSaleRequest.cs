namespace EggERP.Shared.Models
{
    public class CreateSaleItemInput
    {
        public Guid ProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
    public class CreateSaleRequest
    {
        public Guid BusinessId { get; set; }
        public Guid? CustomerId { get; set; }
        public List<CreateSaleItemInput> Items { get; set; } = new();
    }
}