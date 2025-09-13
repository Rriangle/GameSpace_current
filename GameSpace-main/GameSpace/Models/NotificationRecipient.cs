using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Notification_Recipients")]
    public class NotificationRecipient
    {
        [Key]
        public int RecipientId { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; }

        // 導航屬性
        [ForeignKey("NotificationId")]
        public virtual Notification Notification { get; set; } = null!;
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}