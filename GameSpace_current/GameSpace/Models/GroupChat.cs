using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Group_Chat")]
    public class GroupChat
    {
        [Key]
        public int GroupChatId { get; set; }

        [Required]
        public int GroupId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string GroupChatContent { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsSent { get; set; } = true;

        // 導航屬性
        [ForeignKey("GroupId")]
        public virtual Group Group { get; set; } = null!;
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;
    }
}