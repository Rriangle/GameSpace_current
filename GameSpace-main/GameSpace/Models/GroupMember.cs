using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Group_Member")]
    public class GroupMember
    {
        [Key]
        public int GroupId { get; set; }

        [Key]
        public int UserId { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsAdmin { get; set; } = false;

        // 導航屬性
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}