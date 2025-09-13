using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Chat_Message")]
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }

        public int? ManagerId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(255)]
        public string ChatContent { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Required]
        public bool IsRead { get; set; } = false;

        [Required]
        public bool IsSent { get; set; } = true;

        // 導航屬性
        [ForeignKey("ManagerId")]
        public virtual ManagerData? ManagerData { get; set; }
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; } = null!;
        [ForeignKey("ReceiverId")]
        public virtual User Receiver { get; set; } = null!;
    }
}