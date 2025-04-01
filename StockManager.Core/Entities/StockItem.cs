using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManager.Core.Entities
{
    [Table("StockItems")]
    public class StockItem
    {
        [Key]
        [StringLength(12)]
        public string Isin { get; set; }

        [Required]
        public string Name { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal")]
        public decimal Price { get; set; }
    }
}
