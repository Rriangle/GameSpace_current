using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("ThreadPosts")]
    public class ThreadPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ThreadId { get; set; }

        [Required]
        public int AuthorUserId { get; set; }

        [Required]
        public string ContentMd { get; set; } = string.Empty;

        public int? ParentPostId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "normal";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("ThreadId")]
        public virtual Thread Thread { get; set; } = null!;
        [ForeignKey("AuthorUserId")]
        public virtual User AuthorUser { get; set; } = null!;
        [ForeignKey("ParentPostId")]
        public virtual ThreadPost? ParentPost { get; set; }
        public virtual ICollection<ThreadPost> ChildPosts { get; set; } = new List<ThreadPost>();
    }
}