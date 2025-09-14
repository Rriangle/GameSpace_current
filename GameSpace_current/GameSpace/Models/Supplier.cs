using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Supplier")]
    public class Supplier
    {
        [Key]
        public int SupplierId { get; set; }

        [Required]
        [StringLength(100)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}