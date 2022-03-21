using Azure;
using Azure.Data.Tables;
using ProductReviewService.Models;

namespace ProductReviewService.Services
{
    public class ProductsAndReviewsTableService
    {
        private readonly TableClient _tableClient;

        public ProductsAndReviewsTableService(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public IEnumerable<ReviewModel> GetAllReviews()
        {
            Pageable<TableEntity> entities = _tableClient.Query<TableEntity>();

            return entities.Select(MapTableEntityToReviewModel);
        }

        public IEnumerable<ReviewModel> GetReviewsForProduct(string product)
        {
            string filter = $"PartitionKey eq '{product}'";

            Pageable<TableEntity> entities = _tableClient.Query<TableEntity>(filter);

            return entities.Select(MapTableEntityToReviewModel);
        }

        public void InsertTableEntity(ReviewInputModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ReviewText))
            {
                string invertedTicks = ToInvertedTicks(DateTime.UtcNow);

                TableEntity entity = new TableEntity
                {
                    PartitionKey = model.ProductName,
                    RowKey = invertedTicks + Guid.NewGuid().ToString()
                };


                int maxLength = 500;
                entity[nameof(ReviewModel.ReviewText)] = model.ReviewText.Substring(0, Math.Min(maxLength, model.ReviewText.Length));

                _tableClient.AddEntity(entity);
            }
        }

        private string ToInvertedTicks(DateTime dateTime)
        {
            return string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);
        }

        private DateTime ToDateTime(string invertedTicks)
        {
            return new DateTime(DateTime.MaxValue.Ticks - long.Parse(invertedTicks));
        }

        public ReviewModel MapTableEntityToReviewModel(TableEntity entity)
        {
            ReviewModel review = new ReviewModel
            {
                ProductName = entity.PartitionKey,
                Timestamp = ToDateTime(entity.RowKey.Substring(0, 19)),
                ID = Guid.Parse(entity.RowKey.Substring(19, 36)),
                ReviewText = entity[nameof(ReviewModel.ReviewText)]?.ToString()
            };

            return review;
        }
    }
}
