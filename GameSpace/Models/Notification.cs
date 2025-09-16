using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Notifications")]
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public int SourceId { get; set; }

        [Required]
        public int ActionId { get; set; }

        public int? SenderId { get; set; }

        public int? SenderManagerId { get; set; }

        [Required]
        [StringLength(200)]
        public string NotificationTitle { get; set; } = string.Empty;

        [Required]
        public string NotificationMessage { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? GroupId { get; set; }

        // 導航屬性
        [ForeignKey("SourceId")]
        public virtual NotificationSource NotificationSource { get; set; } = null!;
        [ForeignKey("ActionId")]
        public virtual NotificationAction NotificationAction { get; set; } = null!;
        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
        [ForeignKey("SenderManagerId")]
        public virtual ManagerData? SenderManager { get; set; }
        [ForeignKey("GroupId")]
        public virtual Group? Group { get; set; }
        public virtual ICollection<NotificationRecipient> NotificationRecipients { get; set; } = new List<NotificationRecipient>();
    }
}