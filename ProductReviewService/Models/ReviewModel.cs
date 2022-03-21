namespace ProductReviewService.Models
{
    public class ReviewModel
    {
        public string ProductName { get; set; }
        public Guid ID { get; set; }
        public string ReviewText { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
