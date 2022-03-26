using Microsoft.Azure.Cosmos.Table;
using ProductReviewService.Models;

namespace ProductReviewService.Services
{
    public class ProductsAndReviewsTableService
    {
        private readonly CloudTable _productsTable;
        private readonly CloudTable _reviewsTable;

        public ProductsAndReviewsTableService(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("ProductsAndReviewsConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _productsTable = tableClient.GetTableReference("Products");
            _reviewsTable = tableClient.GetTableReference("ProductsAndReviews");
        }

        public IEnumerable<string> GetAllProducts()
        {
            TableQuery<TableEntity> query = new TableQuery<TableEntity>();

            return _productsTable.ExecuteQuery(query).Select(_ => _.PartitionKey);
        }

        public IEnumerable<ReviewModel> GetReviewsForProduct(string product, string nextRowKey = null)
        {
            string filter = $"PartitionKey eq '{product}'";
            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>();
            query.FilterString = filter;

            return _reviewsTable.ExecuteQuery(query).Select(EntityModelToReviewModel);
        }

        public void InsertTableEntity(ReviewInputModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.ReviewText))
            {
                string invertedTicks = ToInvertedTicks(DateTime.UtcNow);

                ReviewEntity entity = new ReviewEntity
                {
                    PartitionKey = model.ProductName,
                    RowKey = invertedTicks + Guid.NewGuid().ToString()
                };


                int maxLength = 500;
                entity.ReviewText = model.ReviewText.Substring(0, Math.Min(maxLength, model.ReviewText.Length));

                _reviewsTable.Execute(TableOperation.Insert(entity));
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

        public ReviewModel EntityModelToReviewModel(ReviewEntity entityModel)
        {
            ReviewModel review = new ReviewModel
            {
                ProductName = entityModel.PartitionKey,
                CreationDateTime = ToDateTime(entityModel.RowKey.Substring(0, 19)),
                ID = Guid.Parse(entityModel.RowKey.Substring(19, 36)),
                ReviewText = entityModel.ReviewText?.ToString()
            };

            return review;
        }
    }

    public class ReviewEntity : TableEntity
    {
        public string ReviewText { get; set; }
    }
}
