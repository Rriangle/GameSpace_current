using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucher")]
    public class EVoucher
    {
        [Key]
        [Column("EVoucherID")]
        public int EVoucherID { get; set; }

        [Required]
        [StringLength(50)]
        [Column("EVoucherCode", TypeName = "varchar(50)")]
        public string EVoucherCode { get; set; } = string.Empty;

        [Required]
        [Column("EVoucherTypeID")]
        public int EVoucherTypeID { get; set; }

        [Required]
        [Column("UserID")]
        public int UserID { get; set; }

        [Required]
        [Column("IsUsed", TypeName = "bit")]
        public bool IsUsed { get; set; } = false;

        [Required]
        [Column("AcquiredTime", TypeName = "datetime2")]
        public DateTime AcquiredTime { get; set; } = DateTime.UtcNow;

        [Column("UsedTime", TypeName = "datetime2")]
        public DateTime? UsedTime { get; set; }

        // 導航屬性
        [ForeignKey("EVoucherTypeID")]
        public virtual EVoucherType EVoucherType { get; set; } = null!;
        [ForeignKey("UserID")]
        public virtual User User { get; set; } = null!;
    }
}