namespace EggERP.Shared.Models
{
    public class InventoryBoardRowDto
    {
        public Guid ProductId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public decimal SellingPrice { get; set; }
        public int Available { get; set; }
        public int ReorderLevel { get; set; }
        public int BoughtAsOfDate { get; set; }
        public int SoldAsOfDate { get; set; }
    }
}