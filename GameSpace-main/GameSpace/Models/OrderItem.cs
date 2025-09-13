using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int LineNo { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; } = 0;

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; } = 0;

        // 導航屬性
        [ForeignKey("OrderId")]
        public virtual OrderInfo OrderInfo { get; set; } = null!;
        [ForeignKey("ProductId")]
        public virtual ProductInfo ProductInfo { get; set; } = null!;
    }
}