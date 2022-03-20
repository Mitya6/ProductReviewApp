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

        public IEnumerable<ReviewModel> GetAllRows()
        {
            Pageable<TableEntity> entities = _tableClient.Query<TableEntity>();

            return entities.Select(MapTableEntityToReviewModel);
        }

        public void InsertTableEntity(ReviewInputModel model)
        {
            TableEntity entity = new TableEntity
            {
                PartitionKey = model.ProductName,
                RowKey = Guid.NewGuid().ToString()
            };

            entity[nameof(ReviewModel.ReviewText)] = model.ReviewText;

            _tableClient.AddEntity(entity);
        }

        public ReviewModel MapTableEntityToReviewModel(TableEntity entity)
        {
            ReviewModel review = new ReviewModel
            {
                ProductName = entity.PartitionKey,
                ID = Guid.Parse(entity.RowKey),
                Timestamp = entity.Timestamp,
                Etag = entity.ETag.ToString(),
                ReviewText = entity[nameof(ReviewModel.ReviewText)].ToString()
            };

            return review;
        }
    }
}
