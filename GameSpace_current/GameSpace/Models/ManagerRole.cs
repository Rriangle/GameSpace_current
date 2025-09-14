using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("ManagerRole")]
    public class ManagerRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public int ManagerId { get; set; }

        [Required]
        public int RolePermissionId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        [ForeignKey("ManagerId")]
        public virtual ManagerData ManagerData { get; set; } = null!;
        [ForeignKey("RolePermissionId")]
        public virtual ManagerRolePermission ManagerRolePermission { get; set; } = null!;
    }
}