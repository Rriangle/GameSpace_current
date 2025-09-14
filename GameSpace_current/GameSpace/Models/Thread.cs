using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Threads")]
    public class Thread
    {
        [Key]
        public int ThreadId { get; set; }

        [Required]
        public int ForumId { get; set; }

        [Required]
        public int AuthorUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "normal";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("ForumId")]
        public virtual Forum Forum { get; set; } = null!;
        [ForeignKey("AuthorUserId")]
        public virtual User AuthorUser { get; set; } = null!;
        public virtual ICollection<ThreadPost> ThreadPosts { get; set; } = new List<ThreadPost>();
    }
}