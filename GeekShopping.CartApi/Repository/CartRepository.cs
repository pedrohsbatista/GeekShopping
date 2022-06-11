﻿using AutoMapper;
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
            var cartHeader = await _mySqlContext.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId);

            if (cartHeader != null)
            {
                _mySqlContext.CartDetails.RemoveRange(_mySqlContext.CartDetails.Where(x => x.CartHeaderId == cartHeader.Id));

                _mySqlContext.CartHeaders.Remove(cartHeader);

                await _mySqlContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public async Task<CartVO> FindCartByUserId(string userId)
        {
            Cart cart = new()
            {
                CartHeader = await _mySqlContext.CartHeaders.FirstOrDefaultAsync(x => x.UserId == userId) ?? new CartHeader()
            };

            cart.CartDetails = _mySqlContext.CartDetails.Where(x => x.CartHeaderId == cart.CartHeader.Id)
                                                              .Include(x => x.Product);

            return _mapper.Map<CartVO>(cart);
        }

        public async Task<bool> RemoveCoupon(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveFromCart(long cartDetailsId)
        {
            try
            {
                CartDetail cartDetail = await _mySqlContext.CartDetails.FirstOrDefaultAsync(x => x.Id == cartDetailsId);

                int total = _mySqlContext.CartDetails.Where(x => x.CartHeaderId == cartDetail.CartHeaderId).Count();

                _mySqlContext.CartDetails.Remove(cartDetail);

                if (total == 1)
                {
                    var cartHeaderToRemove = await _mySqlContext.CartHeaders.FirstOrDefaultAsync(x => x.Id == cartDetail.CartHeaderId);
                    _mySqlContext.CartHeaders.Remove(cartHeaderToRemove);                    
                }

                await _mySqlContext.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;                
            }
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
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeader.Id;
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
