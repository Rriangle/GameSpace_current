using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucherRedeemLog")]
    public class EVoucherRedeemLog
    {
        [Key]
        public int RedeemId { get; set; }

        [Required]
        public int EVoucherId { get; set; }

        public int? TokenId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // "Approved", "Rejected", "Expired", "AlreadyUsed", "Revoked"

        // 導航屬性
        [ForeignKey("EVoucherId")]
        public virtual EVoucher EVoucher { get; set; } = null!;
        [ForeignKey("TokenId")]
        public virtual EVoucherToken? EVoucherToken { get; set; }
    }
}