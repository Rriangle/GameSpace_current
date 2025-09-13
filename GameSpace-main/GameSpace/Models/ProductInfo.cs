using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("ProductInfo")]
    public class ProductInfo
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(200)]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ProductType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; } = 0;

        [Required]
        [StringLength(10)]
        public string CurrencyCode { get; set; } = "NTD";

        [Required]
        public int ShipmentQuantity { get; set; } = 0;

        [StringLength(100)]
        public string? ProductCreatedBy { get; set; }

        [Required]
        public DateTime ProductCreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? ProductUpdatedBy { get; set; }

        [Required]
        public DateTime ProductUpdatedAt { get; set; } = DateTime.UtcNow;

        public int? UserId { get; set; }

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}