using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Notification_Actions")]
    public class NotificationAction
    {
        [Key]
        public int ActionId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActionName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        // 導航屬性
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}