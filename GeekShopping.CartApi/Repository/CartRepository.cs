using AutoMapper;
using GeekShopping.CartApi.Data.ValueObjects;
using GeekShopping.CartApi.Model;
using GeekShopping.CartApi.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace GeekShopping.CartApi.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly MySqlContext _mySqlContext;
        private IMapper _mapper;

        public CartRepository(MySqlContext mySqlContext, IMapper mapper)
        {
            _mySqlContext = mySqlContext;
            _mapper = mapper;
        }

        public async Task<bool> ApplyCoupon(string userId, long couponCode)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> ClearCart(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFromCart(long cartDetailsId)
        {
            throw new NotImplementedException();
        }

        public async Task<CartVO> SaveOrUpdateCart(CartVO cartVO)
        {
            Cart cart = _mapper.Map<Cart>(cartVO);

            var product = await _mySqlContext.Products.FirstOrDefaultAsync(x => x.Id == cart.CartDetails.FirstOrDefault().ProductId);

            if (product == null)
            {
                _mySqlContext.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _mySqlContext.SaveChangesAsync();
            }

            var cartHeader = await _mySqlContext.CartHeaders.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == cart.CartHeader.UserId);

            if (cartHeader == null)
            {
                _mySqlContext.CartHeaders.Add(cart.CartHeader);
                await _mySqlContext.SaveChangesAsync();
                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                cart.CartDetails.FirstOrDefault().Product = null;
                _mySqlContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _mySqlContext.SaveChangesAsync();
            }
            else
            {
                var cartDetails = await _mySqlContext.CartDetails.AsNoTracking()
                                                .FirstOrDefaultAsync(x => x.ProductId == cart.CartDetails.FirstOrDefault().ProductId
                                                                     && x.CartHeaderId == cartHeader.Id);
                if (cartDetails == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _mySqlContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _mySqlContext.SaveChangesAsync();
                }
                else
                {
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetails.Count;
                    cart.CartDetails.FirstOrDefault().Id = cartDetails.Id;
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetails.CartHeaderId;
                    _mySqlContext.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _mySqlContext.SaveChangesAsync();
                }
            }

            return _mapper.Map<CartVO>(cart);
        }
    }
}
