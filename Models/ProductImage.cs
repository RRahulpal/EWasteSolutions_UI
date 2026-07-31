using System.ComponentModel.DataAnnotations;

namespace EWasteSolutions.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [StringLength(1000)]
        [Display(Name = "Image URL")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Cloudinary Public ID")]
        public string? PublicId { get; set; }

        [StringLength(250)]
        [Display(Name = "Alternative Text")]
        public string? AltText { get; set; }

        [Display(Name = "Primary Image")]
        public bool IsPrimary { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Product? Product { get; set; }
    }
}