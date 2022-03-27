using System.ComponentModel.DataAnnotations;

namespace ProductReviewService.Models
{
    public class ReviewInputModel
    {
        [Required]
        public string ProductName { get; set; }

        [Required]
        [StringLength(500)]
        public string ReviewText { get; set; }

        [StringLength(500)]
        public string LatestReviewText { get; set; }
    }
}
