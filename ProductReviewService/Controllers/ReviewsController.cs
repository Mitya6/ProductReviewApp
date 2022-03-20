using Microsoft.AspNetCore.Mvc;
using ProductReviewService.Models;
using ProductReviewService.Services;

namespace ProductReviewService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly ProductsAndReviewsTableService _tableService;

        public ReviewsController(ProductsAndReviewsTableService tableService)
        {
            _tableService = tableService;
        }

        // GET: api/<ReviewsController>
        [HttpGet]
        public IEnumerable<ReviewModel> Get()
        {
            return _tableService.GetAllReviews().ToArray();
        }

        // GET api/<ReviewsController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        // POST api/<ReviewsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            _tableService.InsertTableEntity(new ReviewInputModel
            {
                ProductName = "Lego",
                ReviewText = "some text"
            });
        }

        // PUT api/<ReviewsController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/<ReviewsController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
