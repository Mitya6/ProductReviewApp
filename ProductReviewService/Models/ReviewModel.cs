using Microsoft.Azure.Cosmos.Table;

namespace ProductReviewService.Models
{
    public class ReviewModel
    {
        public string ProductName { get; set; }
        public string ReviewText { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}
