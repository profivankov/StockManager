namespace StockManager.Core.DTOs
{
    public class StockItemDTO
    {
        public string Isin { get; set; }
        public string Name { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
    }
}
