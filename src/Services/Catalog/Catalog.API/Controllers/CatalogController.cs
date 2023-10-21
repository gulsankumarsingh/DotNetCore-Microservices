using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _repository.GetProducts();

            return Ok(products);
        }

        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(string id)
        {
            var product = await _repository.GetProduct(id);

            if(product == null)
            {
                _logger.LogError($"Product with id: {id}, not found");
                return NotFound($"Product with id: {id}, not found");
            }

            return Ok(product);
        }

        [HttpGet("{category}", Name = "GetProductByCategory")]
        //[HttpGet("[action/{category}]", Name = "GetProductByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductByCategory(string category)
        {
            var products = await _repository.GetProductByCategory(category);

            if (products == null)
            {
                _logger.LogError($"Product with category: {category}, not found");
                return NotFound($"Product with category: {category}, not found");
            }

            return Ok(products);
        }

        [HttpGet("{name}", Name = "GetProductByName")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<Product>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProductByName(string name)
        {
            var products = await _repository.GetProductByName(name);

            if (products == null)
            {
                _logger.LogError($"Product with name: {name}, not found");
                return NotFound();
            }

            return Ok(products);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody]Product product)
        {
            if (product == null)
            {
                _logger.LogError("Invalid product provided while creating the product");
                return BadRequest();
            }

            await _repository.CreateProduct(product);
            return CreatedAtRoute("GetProduct", new {id = product.Id}, product);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProduct(Product product)
        {

            var isUpdateSuccess = await _repository.UpdateProduct(product);
            if (isUpdateSuccess)
            {
                return Ok("Product Updated Successfully");
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpDelete("{id:length(24)}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            var product = await _repository.GetProduct(id);

            if (product == null)
            {
                _logger.LogError($"Product not found with id: {id}");
                return BadRequest($"Product not found with id: {id}");
            }
            var isUpdateSuccess = await _repository.DeleteProduct(id);
            if (isUpdateSuccess)
            {
                return Ok("Product Deleted Successfully");
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
