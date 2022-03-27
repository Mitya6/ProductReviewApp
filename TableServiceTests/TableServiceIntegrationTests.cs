using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductReviewService.Services;
using System.Linq;
using System.Reflection;

namespace TableServiceTests
{
    [TestClass]
    public class TableServiceIntegrationTests
    {
        private static ProductsAndReviewsTableService _tableService;
        private static CloudTable _reviewsTable;

        private const string _testProductName = "TestProduct";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // TODO: we could use the StorageEmulator instead of live tables


            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddUserSecrets(Assembly.GetAssembly(typeof(ProductsAndReviewsTableService)));

            var configuration = builder.Build();


            var storageAccount = CloudStorageAccount.Parse(configuration.GetConnectionString("ProductsAndReviewsConnectionString"));
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            var productsTable = tableClient.GetTableReference("Products");
            _reviewsTable = tableClient.GetTableReference("ReviewsTestTable");

            _tableService = new ProductsAndReviewsTableService(productsTable, _reviewsTable);
        }

        [TestMethod]
        public void CanInsertReview()
        {
            var testReviewText = "some review text";

            _tableService.InsertReviewEntity(_testProductName, testReviewText);

            TableQuery<TableEntity> query = new TableQuery<TableEntity>
            {
                FilterString = QueryHelpers.StartsWithFilter(_testProductName)
            };

            var reviews = _reviewsTable.ExecuteQuery(query).ToArray();

            Assert.AreEqual(1, reviews.Length);
            Assert.AreEqual(testReviewText, reviews.First().RowKey);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TableQuery<TableEntity> query = new TableQuery<TableEntity>
            {
                FilterString = QueryHelpers.StartsWithFilter(_testProductName)
            };

            var reviews = _reviewsTable.ExecuteQuery(query).ToArray();

            foreach (var review in reviews)
            {
                _reviewsTable.Execute(TableOperation.Delete(review));
            }
        }
    }
}