using System.ComponentModel.DataAnnotations;

namespace StockManager.Core.Entities
{
    public class StockItem
    {
        [Key]
        public string Isin { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
