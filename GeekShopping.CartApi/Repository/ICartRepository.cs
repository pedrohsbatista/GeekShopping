using GeekShopping.CartApi.Data.ValueObjects;

namespace GeekShopping.CartApi.Repository
{
    public interface ICartRepository
    {
        Task<CartVO> FindCartByUserId(string userId);

        Task<CartVO> SaveOrUpdateCart(CartVO cartVO);

        Task<bool> RemoveFromCart(long cartDetailsId);

        Task<bool> ApplyCoupon(string userId, long couponCode);

        Task<bool> RemoveCoupon(string userId);

        Task<bool> ClearCart(string userId);
    }
}
