using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;
using System.Net.Http.Headers;

namespace GeekShopping.Web.Services
{
    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;
        public const string BasePath = "api/v1/cart";

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<CartViewModel> AddItemToCart(CartViewModel cart, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJson($"{BasePath}/add-cart", cart);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something went wrong calling API");

            return await response.ReadContentAs<CartViewModel>();
        }

        public async Task<bool> ApplyCoupon(CartViewModel cartViewModel, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJson($"{BasePath}/apply-coupon", cartViewModel);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something went wrong calling API");

            return await response.ReadContentAs<bool>();
        }

        public async Task<object> Checkout(CartHeaderViewModel cartHeaderViewModel, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsJson($"{BasePath}/checkout", cartHeaderViewModel);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode.ToString().Equals("PreconditionFailed"))
                    return "Coupon price has changed, please confirm";
                
                throw new Exception("Something went wrong when calling API");
            }                

            return await response.ReadContentAs<CartHeaderViewModel>();
        }

        public async Task<bool> ClearCart(string userId, string token)
        {
            throw new NotImplementedException();
        }

        public async Task<CartViewModel> FindCartByUserId(string userId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{BasePath}/find-cart/{userId}");
            return await response.ReadContentAs<CartViewModel>();
        }

        public async Task<bool> RemoveCoupon(string userId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"{BasePath}/remove-coupon/{userId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something went wrong when calling API");

            return await response.ReadContentAs<bool>();
        }

        public async Task<bool> RemoveFromCart(long cartId, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.DeleteAsync($"{BasePath}/remove-cart/{cartId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something wrong calling API");

            return await response.ReadContentAs<bool>();
        }

        public async Task<CartViewModel> UpdateCart(CartViewModel cart, string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PutAsJson($"{BasePath}/update-cart", cart);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something wrong calling API");

            return await response.ReadContentAs<CartViewModel>();
        }
    }
}
