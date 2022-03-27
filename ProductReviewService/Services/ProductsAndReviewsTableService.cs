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
            _reviewsTable = tableClient.GetTableReference("Reviews");
        }

        public ProductsAndReviewsTableService(CloudTable productsTable, CloudTable reviewsTable)
        {
            _productsTable = productsTable;
            _reviewsTable = reviewsTable;
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

        public ReviewModel GetLatestReview(string product)
        {
            if (string.IsNullOrEmpty(product))
            {
                throw new ArgumentException("Product cannot be null or empty.");
            }

            TableQuery<TableEntity> query = new TableQuery<TableEntity>
            {
                FilterString = QueryHelpers.StartsWithFilter(product),
                TakeCount = 1
            };

            return _reviewsTable.ExecuteQuery(query).Select(TableEntityToReviewModel).FirstOrDefault();
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


            TableQuery<TableEntity> query = new TableQuery<TableEntity>
            {
                FilterString = QueryHelpers.StartsWithFilter(product),
                TakeCount = chunkSize
            };

            var tableQuerySegment = _reviewsTable.ExecuteQuerySegmented(query, continuationToken);
            
            return new ChunkedResult<ReviewModel>
            {
                ChunkSize = chunkSize,
                ContinuationToken = tableQuerySegment.ContinuationToken,
                Results = tableQuerySegment.Results.Select(TableEntityToReviewModel).ToArray()
            };
        }

        public TableResult InsertReviewEntity(string product, string reviewText)
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

            TableEntity entity = new TableEntity
            {
                PartitionKey = product + invertedTicks,
                RowKey = reviewText
            };

            return _reviewsTable.Execute(TableOperation.Insert(entity));
        }

        private string ToInvertedTicks(DateTime dateTime)
        {
            return string.Format("{0:D19}", DateTime.MaxValue.Ticks - dateTime.Ticks);
        }

        private DateTime ToDateTime(string invertedTicks)
        {
            return new DateTime(DateTime.MaxValue.Ticks - long.Parse(invertedTicks));
        }

        public ReviewModel TableEntityToReviewModel(TableEntity entity)
        {
            return new ReviewModel
            {
                ProductName = entity.PartitionKey.Substring(0, entity.PartitionKey.Length - 19),
                CreationDateTime = ToDateTime(entity.PartitionKey.Substring(entity.PartitionKey.Length - 19)),
                ReviewText = entity.RowKey
            };
        }
    }
}
