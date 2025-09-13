using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("User_Introduce")]
    public class UserIntroduce
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserNickName { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string IdNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Cellphone { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required]
        public DateTime CreateAccount { get; set; } = DateTime.UtcNow;

        public byte[]? UserPicture { get; set; }

        [StringLength(200)]
        public string? UserIntroduceText { get; set; }

        // 導航屬性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}