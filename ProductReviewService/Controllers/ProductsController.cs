using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using ProductReviewService.Models;
using ProductReviewService.Services;

namespace ProductReviewService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsAndReviewsTableService _tableService;

        public ProductsController(ProductsAndReviewsTableService tableService)
        {
            _tableService = tableService;
        }

        // GET: api/<ProductsController>
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            try
            {
                return _tableService.GetAllProducts().ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        //GET api/<ProductsController>/5
        [HttpGet("{product}")]
        public ActionResult<ChunkedResult<ReviewModel>> Get(string product, string nextPartitionKey, string nextRowKey, string nextTableName, int targetLocation)
        {
            try
            {
                if (!_tableService.ProductExists(product))
                {
                    return BadRequest();
                }

                TableContinuationToken continuationToken = null;

                if (!string.IsNullOrEmpty(nextPartitionKey) && !string.IsNullOrEmpty(nextRowKey) && Enum.IsDefined(typeof(StorageLocation), targetLocation))
                {
                    continuationToken = new TableContinuationToken
                    {
                        NextPartitionKey = nextPartitionKey,
                        NextRowKey = nextRowKey,
                        NextTableName = nextTableName,
                        TargetLocation = (StorageLocation)targetLocation
                    };
                }

                return _tableService.GetReviewsChunk(product, 5, continuationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        // POST api/<ProductsController>
        [HttpPost]
        public ActionResult Post([FromBody] ReviewInputModel model)
        {
            try
            {
                if (!_tableService.ProductExists(model.ProductName))
                {
                    return BadRequest();
                }

                if (!IsFirstOrContainsLatestReview(model))
                {
                    return BadRequest();
                }

                _tableService.InsertTableEntity(model.ProductName, model.ReviewText);

                return StatusCode(StatusCodes.Status201Created);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private bool IsFirstOrContainsLatestReview(ReviewInputModel model)
        {
            ReviewModel latestReview = _tableService.GetLatestReview(model.ProductName);

            return (latestReview == null && string.IsNullOrEmpty(model.LatestReviewText)) ||
                (latestReview != null && string.Equals(latestReview.ReviewText, model.LatestReviewText, StringComparison.Ordinal));
        }
    }
}
