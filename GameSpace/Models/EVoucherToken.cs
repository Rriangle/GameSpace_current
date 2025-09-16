using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucherToken")]
    public class EVoucherToken
    {
        [Key]
        [Column("TokenID")]
        public int TokenID { get; set; }

        [Required]
        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(64)]
        [Column("Token", TypeName = "varchar(64)")]
        public string Token { get; set; } = string.Empty;

        [Required]
        [Column("ExpiresAt", TypeName = "datetime2")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

        [Required]
        [Column("IsRevoked", TypeName = "bit")]
        public bool IsRevoked { get; set; } = false;

        // 導航屬性
        [ForeignKey("EVoucherID")]
        public virtual EVoucher EVoucher { get; set; } = null!;
    }
}