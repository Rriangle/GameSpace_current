using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Groups")]
    public class Group
    {
        [Key]
        public int GroupId { get; set; }

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;
        public virtual ICollection<GroupMember> GroupMembers { get; set; } = new List<GroupMember>();
        public virtual ICollection<GroupChat> GroupChats { get; set; } = new List<GroupChat>();
        public virtual ICollection<GroupBlock> GroupBlocks { get; set; } = new List<GroupBlock>();
    }
}