using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Group_Block")]
    public class GroupBlock
    {
        [Key]
        public int BlockId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BlockedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("BlockedBy")]
        public virtual User BlockedByUser { get; set; } = null!;
    }
}