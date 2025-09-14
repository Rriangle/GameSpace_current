using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        public int EVoucherId { get; set; }

        [Required]
        [StringLength(100)]
        public string EVoucherCode { get; set; } = string.Empty;

        [Required]
        public int EVoucherTypeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsUsed { get; set; } = false;

        [Required]
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;

        public DateTime? UsedTime { get; set; }

        // 導航屬性
        [ForeignKey("EVoucherTypeId")]
        public virtual EVoucherType EVoucherType { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}