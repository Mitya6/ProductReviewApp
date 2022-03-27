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

        public bool ProductExists(string product)
        {
            TableQuery<TableEntity> query = new TableQuery<TableEntity>
            {
                FilterString = $"PartitionKey eq '{product}'",
                TakeCount = 1
            };

            return _productsTable.ExecuteQuery(query).Count() == 1;
        }

        public ChunkedResult<ReviewModel> GetReviewsChunk(string product, int chunkSize, TableContinuationToken continuationToken)
        {
            if (string.IsNullOrEmpty(product))
            {
                throw new ArgumentException("Product cannot be null or empty.");
            }
            if (chunkSize < 1 || chunkSize > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Must be between 1 and 1000 inclusive.");
            }

            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>
            {
                FilterString = $"PartitionKey eq '{product}'",
                TakeCount = chunkSize
            };

            var tableQuerySegment = _reviewsTable.ExecuteQuerySegmented(query, continuationToken);
            
            return new ChunkedResult<ReviewModel>
            {
                ChunkSize = chunkSize,
                ContinuationToken = tableQuerySegment.ContinuationToken,
                Results = tableQuerySegment.Results.Select(ReviewEntityToReviewModel).ToArray()
            };
        }


        public void InsertReviewEntity(string product, string reviewText)
        {
            if (string.IsNullOrEmpty(product))
            {
                throw new ArgumentException("Product cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(reviewText))
            {
                throw new ArgumentException("Review text cannot be null or empty.");
            }
            if (reviewText.Length > 500)
            {
                throw new ArgumentException("The length of the Review text must not be more than 500 characters.");
            }

            string invertedTicks = ToInvertedTicks(DateTime.UtcNow);

            ReviewEntity entity = new ReviewEntity
            {
                PartitionKey = product,
                RowKey = invertedTicks + Guid.NewGuid().ToString(),
                ReviewText = reviewText
            };

            _reviewsTable.Execute(TableOperation.Insert(entity));
        }

        public ReviewModel GetLatestReview(string product)
        {
            if (string.IsNullOrEmpty(product))
            {
                throw new ArgumentException("Product cannot be null or empty.");
            }

            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>
            {
                FilterString = $"PartitionKey eq '{product}'",
                TakeCount = 1
            };

            return _reviewsTable.ExecuteQuery(query).Select(ReviewEntityToReviewModel).FirstOrDefault();
        }

        private string ToInvertedTicks(DateTime dateTime)
        {
            return string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);
        }

        private DateTime ToDateTime(string invertedTicks)
        {
            return new DateTime(DateTime.MaxValue.Ticks - long.Parse(invertedTicks));
        }

        public ReviewModel ReviewEntityToReviewModel(ReviewEntity entityModel)
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
}
