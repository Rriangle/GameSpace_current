using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("PlayerMarketOrderInfo")]
    public class PlayerMarketOrderInfo
    {
        [Key]
        public int POrderId { get; set; }

        [Required]
        public int PProductId { get; set; }

        [Required]
        public int SellerId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public DateTime POrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string POrderStatus { get; set; } = "交易中";

        [Required]
        [StringLength(20)]
        public string PPaymentStatus { get; set; } = "待付款";

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PUnitPrice { get; set; } = 0;

        [Required]
        public int PQuantity { get; set; } = 1;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal POrderTotal { get; set; } = 0;

        [Required]
        public DateTime POrderCreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime POrderUpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("PProductId")]
        public virtual PlayerMarketProductInfo PlayerMarketProductInfo { get; set; } = null!;
        [ForeignKey("SellerId")]
        public virtual User Seller { get; set; } = null!;
        [ForeignKey("BuyerId")]
        public virtual User Buyer { get; set; } = null!;
    }
}