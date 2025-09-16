using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Posts")]
    public class Post
    {
        [Key]
        public int PostId { get; set; }

        [Required]
        [StringLength(20)]
        public string Type { get; set; } = string.Empty; // "insight", "user"

        public int? GameId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Tldr { get; set; }

        [Required]
        public string BodyMd { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "draft";

        [Required]
        public bool Pinned { get; set; } = false;

        [Required]
        public int CreatedBy { get; set; }

        public DateTime? PublishedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("CreatedBy")]
        public virtual User CreatedByUser { get; set; } = null!;
    }
}