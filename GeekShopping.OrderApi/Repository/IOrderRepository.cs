using GeekShopping.OrderApi.Model;

namespace GeekShopping.OrderApi.Repository
{
    public interface IOrderRepository
    {
        Task<bool> AddOrder(OrderHeader orderHeader);

        Task UpdateOrderPaymentStatus(long orderHeaderId, bool status);
    }
}
