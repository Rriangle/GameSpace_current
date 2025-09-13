using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("PlayerMarketTradeMsg")]
    public class PlayerMarketTradeMsg
    {
        [Key]
        public int TradeMsgId { get; set; }

        [Required]
        public int POrderTradepageId { get; set; }

        [Required]
        [StringLength(20)]
        public string MsgFrom { get; set; } = string.Empty; // "seller", "buyer"

        [Required]
        public string MessageText { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("POrderTradepageId")]
        public virtual PlayerMarketOrderTradepage PlayerMarketOrderTradepage { get; set; } = null!;
    }
}