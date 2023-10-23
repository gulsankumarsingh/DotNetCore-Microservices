using Discount.API.Entities;
using Discount.API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountRepository _discountRepository;
        private readonly ILogger<DiscountController> _logger;

        public DiscountController(IDiscountRepository discountRepository, ILogger<DiscountController> logger)
        {
            _discountRepository = discountRepository;
            _logger = logger;
        }

        [HttpGet("{productName}", Name= "GetDiscount")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Coupon))]
        public async Task<IActionResult> GetDiscount(string productName)
        {
            var coupon = await _discountRepository.GetDiscount(productName);
            return Ok(coupon);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Coupon))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> CreateDiscount([FromBody]Coupon coupon)
        {
            var isSuccess = await _discountRepository.CreateDiscount(coupon);

            if (isSuccess)
            {
                return CreatedAtRoute("GetDiscount", new {productName = coupon.ProductName}, coupon);
            }
            else
            {
                _logger.LogError("An error occured while creating the discount", coupon);
                return StatusCode(StatusCodes.Status500InternalServerError, "Oops, something went wrong while creating the discount");
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> UpdateDiscount([FromBody] Coupon coupon)
        {
            var isSuccess = await _discountRepository.UpdateDiscount(coupon);

            if (isSuccess)
            {
                return Ok("Record updated successfully");
            }
            else
            {
                _logger.LogError("An error occured while updaing the discount", coupon);
                return StatusCode(StatusCodes.Status500InternalServerError, "Oops, something went wrong while updating the discount");
            }
        }

        [HttpDelete("{productName}", Name = "DeleteDiscount")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(string))]
        public async Task<IActionResult> DeleteDiscount(string productName)
        {
            var record = await _discountRepository.GetDiscount(productName);
            if(record.Id == 0)
            {
                return NotFound($"No coupon present for product: {productName}");
            }
            var isSuccess = await _discountRepository.DeleteDiscount(productName);
            if (isSuccess)
            {
                return Ok("Record deleted successfully");
            }
            else
            {
                _logger.LogError("An error occured while deleting the discount", productName);
                return StatusCode(StatusCodes.Status500InternalServerError, "Oops, something went wrong while deleting the discount");
            }
        }
    }
}
