using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Relation")]
    public class Relation
    {
        [Key]
        public int RelationId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int FriendId { get; set; }

        [Required]
        public int StatusId { get; set; }

        [StringLength(50)]
        public string? FriendNickname { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        [ForeignKey("FriendId")]
        public virtual User Friend { get; set; } = null!;
        [ForeignKey("StatusId")]
        public virtual RelationStatus RelationStatus { get; set; } = null!;
    }
}