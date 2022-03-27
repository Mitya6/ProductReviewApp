using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductReviewService.Services;
using System;
using System.Linq;
using System.Reflection;

namespace TableServiceTests
{
    [TestClass]
    public class TableServiceUnitTests
    {
        private static ProductsAndReviewsTableService _tableService;
        private static CloudTable _reviewsTable;

        private const string _longReviewProductName = "LongReviewProduct";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // TODO: mock the tables in unit tests


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
        public void NullOrEmptyProductThrows()
        {
            Assert.ThrowsException<ArgumentException>(() => _tableService.InsertReviewEntity(null, "text"));
            Assert.ThrowsException<ArgumentException>(() => _tableService.InsertReviewEntity(string.Empty, "text"));
        }

        [TestMethod]
        public void NullOrEmptyReviewTextThrows()
        {
            Assert.ThrowsException<ArgumentException>(() => _tableService.InsertReviewEntity("someProduct", null));
            Assert.ThrowsException<ArgumentException>(() => _tableService.InsertReviewEntity("someProduct", string.Empty));
        }

        [TestMethod]
        public void TooLongReviewTextThrows()
        {
            string longInput = new string('a', 501);
            Assert.ThrowsException<ArgumentException>(() => _tableService.InsertReviewEntity("someProduct", longInput));
        }

        [TestMethod]
        public void CanInsertReviewOfMaxSize()
        {
            string longInput = new string('a', 500);
            _tableService.InsertReviewEntity(_longReviewProductName, longInput);

            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>
            {
                FilterString = $"PartitionKey eq '{_longReviewProductName}'"
            };

            var reviews = _reviewsTable.ExecuteQuery(query).ToArray();

            Assert.AreEqual(1, reviews.Length);
            Assert.AreEqual(longInput, reviews.First().ReviewText);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            TableQuery<ReviewEntity> query = new TableQuery<ReviewEntity>
            {
                FilterString = $"PartitionKey eq '{_longReviewProductName}'"
            };

            var reviews = _reviewsTable.ExecuteQuery(query).ToArray();

            foreach (var review in reviews)
            {
                _reviewsTable.Execute(TableOperation.Delete(review));
            }
        }
    }
}