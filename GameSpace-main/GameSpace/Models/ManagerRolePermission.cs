using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("ManagerRolePermission")]
    public class ManagerRolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        public bool UserStatusManagement { get; set; } = false;

        [Required]
        public bool ShoppingPermissionManagement { get; set; } = false;

        [Required]
        public bool MessagePermissionManagement { get; set; } = false;

        [Required]
        public bool PetRightsManagement { get; set; } = false;

        [Required]
        public bool SystemSettingsManagement { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        public virtual ICollection<ManagerRole> ManagerRoles { get; set; } = new List<ManagerRole>();
    }
}