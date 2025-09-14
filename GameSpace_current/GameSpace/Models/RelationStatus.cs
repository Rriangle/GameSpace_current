using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Relation_Status")]
    public class RelationStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        [StringLength(20)]
        public string StatusName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Description { get; set; }

        // 導航屬性
        public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();
    }
}