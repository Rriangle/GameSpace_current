using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucherToken")]
    public class EVoucherToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required]
        public int EVoucherId { get; set; }

        [Required]
        [StringLength(64)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

        [Required]
        public bool IsRevoked { get; set; } = false;

        // 導航屬性
        [ForeignKey("EVoucherId")]
        public virtual EVoucher EVoucher { get; set; } = null!;
    }
}