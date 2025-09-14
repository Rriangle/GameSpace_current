using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("PlayerMarketOrderTradepage")]
    public class PlayerMarketOrderTradePage
    {
        [Key]
        public int POrderTradepageId { get; set; }

        [Required]
        public int POrderId { get; set; }

        [Required]
        public int PProductId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal POrderPlatformFee { get; set; } = 0;

        public DateTime? SellerTransferredAt { get; set; }

        public DateTime? BuyerReceivedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // 導航屬性
        [ForeignKey("POrderId")]
        public virtual PlayerMarketOrderInfo PlayerMarketOrderInfo { get; set; } = null!;
        public virtual ICollection<PlayerMarketTradeMessage> PlayerMarketTradeMsgs { get; set; } = new List<PlayerMarketTradeMessage>();
    }
}