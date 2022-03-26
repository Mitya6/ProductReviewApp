using Microsoft.Azure.Cosmos.Table;
using ProductReviewService.Models;

namespace ProductReviewService.Services
{
    public class ProductsAndReviewsTableService
    {
        private readonly CloudTable _cloudTable;

        public ProductsAndReviewsTableService(IConfiguration configuration)
        {
            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("ProductsAndReviewsConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _cloudTable = tableClient.GetTableReference("ProductsAndReviews");
        }

        public IEnumerable<string> GetAllProducts()
        {
            // TODO: Improve the performance of the below query as it currently does a full table scan.
            //       Maybe list each product in another table as a single partition key and query that table.

            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>();

            return _cloudTable.ExecuteQuery(query).Select(_ => _.PartitionKey).Distinct();
        }

        public IEnumerable<ReviewModel> GetReviewsForProduct(string product, string nextRowKey = null)
        {
            string filter = $"PartitionKey eq '{product}'";
            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>();
            query.FilterString = filter;

            return _cloudTable.ExecuteQuery(query).Select(EntityModelToReviewModel);
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

                _cloudTable.Execute(TableOperation.Insert(entity));
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
