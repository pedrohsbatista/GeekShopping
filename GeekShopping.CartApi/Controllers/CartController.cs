using GeekShopping.CartApi.Data.ValueObjects;
using GeekShopping.CartApi.Messages;
using GeekShopping.CartApi.RabbitMQSender;
using GeekShopping.CartApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace GeekShopping.CartApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private ICartRepository _cartRepository;
        private ICouponRepository _couponRepository;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        public CartController(ICartRepository cartRepository, ICouponRepository couponRepository, IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _couponRepository = couponRepository;
            _rabbitMQMessageSender =  rabbitMQMessageSender ?? throw new ArgumentNullException(nameof(rabbitMQMessageSender));
        }

        [HttpGet("find-cart/{id}")]
        public async Task<ActionResult<CartVO>> FindById(string id)
        {
            var cart = await _cartRepository.FindCartByUserId(id);

            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPost("add-cart")]
        public async Task<ActionResult<CartVO>> AddCart(CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPut("update-cart")]
        public async Task<ActionResult<CartVO>> UpdateCart(CartVO cartVO)
        {
            var cart = await _cartRepository.SaveOrUpdateCart(cartVO);

            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpDelete("remove-cart/{id}")]
        public async Task<ActionResult<CartVO>> RemoveCart(int id)
        {
            var status = await _cartRepository.RemoveFromCart(id);

            if (!status)
                return BadRequest();

            return Ok(status);
        }

        [HttpPost("apply-coupon")]
        public async Task<ActionResult<CartVO>> ApplyCoupon(CartVO cartVO)
        {
            var status = await _cartRepository.ApplyCoupon(cartVO.CartHeader.UserId, cartVO.CartHeader.CouponCode);

            if (!status)
                return NotFound();

            return Ok(status);
        }

        [HttpDelete("remove-coupon/{userId}")]
        public async Task<ActionResult<CartVO>> RemoveCoupon(string userId)
        {
            var status = await _cartRepository.RemoveCoupon(userId);

            if (!status)
                return NotFound();

            return Ok(status);
        }

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutHeaderVO>> Checkout(CheckoutHeaderVO checkoutHeaderVO)
        {
            string token = Request.Headers.Authorization;            

            if (checkoutHeaderVO?.UserId == null)
                return BadRequest();

            var cart = await _cartRepository.FindCartByUserId(checkoutHeaderVO.UserId);

            if (cart == null)
                return NotFound();

            if (!string.IsNullOrEmpty(checkoutHeaderVO.CouponCode))
            {
                CouponVO couponVO = await _couponRepository.GetCoupon(checkoutHeaderVO.CouponCode, token);

                if (couponVO.DiscountAmount != checkoutHeaderVO.DiscountAmount)
                    return StatusCode(412);
            }

            checkoutHeaderVO.CartDetails = cart.CartDetails;
            checkoutHeaderVO.DateTime = DateTime.Now;

            _rabbitMQMessageSender.SendMessage(checkoutHeaderVO, "checkoutQueue");

            return Ok(checkoutHeaderVO);
        }
    }
}
