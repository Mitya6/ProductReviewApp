using Microsoft.Azure.Cosmos.Table;

namespace ProductReviewService.Services
{
    public class ReviewEntity : TableEntity
    {
        public string ReviewText { get; set; }
    }
}
