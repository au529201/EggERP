namespace EggERP.Shared.Models
{
    public class StockBoardRowDto
    {
        public Guid ProductId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Available { get; set; }
        public int Bought { get; set; }
        public int Sold { get; set; }
        public decimal TotalSales { get; set; }
    }
}