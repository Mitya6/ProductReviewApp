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
            return _tableService.GetAllProducts().ToArray();
        }

        //GET api/<ProductsController>/5
        [HttpGet("{id}")]
        public ActionResult<ChunkedResult<ReviewModel>> Get(string id, string nextPartitionKey, string nextRowKey, string nextTableName, int targetLocation)
        {
            // TODO: validation

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


            return _tableService.GetReviewsChunk(id, 5, continuationToken);
        }

        // POST api/<ProductsController>
        [HttpPost]
        public ActionResult Post([FromBody] ReviewInputModel model)
        {
            _tableService.InsertTableEntity(model);

            return Ok();
        }
    }
}
