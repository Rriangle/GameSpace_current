using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameSpace.Models
{
    [Table("PlayerMarketProductImgs")]
    public class PlayerMarketProductImg
    {
        [Key]
        public int PProductImgId { get; set; }

        [Required]
        public int PProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string PProductImgUrl { get; set; } = string.Empty;

        // 導航屬性
        [ForeignKey("PProductId")]
        public virtual PlayerMarketProductInfo PlayerMarketProductInfo { get; set; } = null!;
    }
}