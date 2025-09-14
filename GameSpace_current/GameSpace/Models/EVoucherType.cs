using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("EVoucherType")]
    public class EVoucherType
    {
        [Key]
        public int EVoucherTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValueAmount { get; set; } = 0;

        [Required]
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ValidTo { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required]
        public int PointsCost { get; set; } = 0;

        [Required]
        public int TotalAvailable { get; set; } = 0;

        [StringLength(500)]
        public string? Description { get; set; }

        // 導航屬性
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
    }
}