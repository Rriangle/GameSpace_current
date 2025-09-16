using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucherRedeemLog")]
    public class EVoucherRedeemLog
    {
        [Key]
        [Column("RedeemID")]
        public int RedeemID { get; set; }

        [Required]
        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Column("TokenID")]
        public int? TokenID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = string.Empty; // "Approved", "Rejected", "Expired", "AlreadyUsed", "Revoked"

        // 導航屬性
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
        [ForeignKey("TokenID")]
        public virtual EVoucherToken? EVoucherToken { get; set; }
    }
}