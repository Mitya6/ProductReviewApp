using Microsoft.AspNetCore.Mvc;
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
            return _tableService.GetAllReviews().Select(_ => _.ProductName).Distinct().ToArray();
        }

        //GET api/<ProductsController>/5
        [HttpGet("{id}")]
        public ActionResult<IEnumerable<ReviewModel>> Get(string id)
        {
            return _tableService.GetReviewsForProduct(id).ToArray();
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
