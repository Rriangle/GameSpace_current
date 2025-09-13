using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserAccount { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string UserPassword { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UserEmail { get; set; }

        [StringLength(20)]
        public string? UserPhone { get; set; }

        public bool UserEmailConfirmed { get; set; } = false;

        public bool UserPhoneConfirmed { get; set; } = false;

        public int UserAccessFailedCount { get; set; } = 0;

        public bool UserLockoutEnabled { get; set; } = false;

        public DateTime? UserLockoutEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // 導航屬性
        public virtual UserIntroduce? UserIntroduce { get; set; }
        public virtual UserRights? UserRights { get; set; }
        public virtual UserWallet? UserWallet { get; set; }
        public virtual ICollection<Pet> Pets { get; set; } = new List<Pet>();
        public virtual ICollection<MiniGame> MiniGames { get; set; } = new List<MiniGame>();
        public virtual ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public virtual ICollection<EVoucher> EVouchers { get; set; } = new List<EVoucher>();
        public virtual ICollection<WalletHistory> WalletHistories { get; set; } = new List<WalletHistory>();
        public virtual ICollection<UserSignInStats> UserSignInStats { get; set; } = new List<UserSignInStats>();
    }
}