using GeekShopping.CartApi.Data.ValueObjects;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GeekShopping.CartApi.Repository
{
    public class CouponRepository : ICouponRepository 
    {
        private readonly HttpClient _httpClient;

        public CouponRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<CouponVO> GetCoupon(string couponCode, string token)
        {
            var tokenParts = token.Split(" ");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(tokenParts.FirstOrDefault(), tokenParts.LastOrDefault());
            var response = await _httpClient.GetAsync($"/api/v1/coupon/{couponCode}");
            var content = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
                return new CouponVO();

            return JsonSerializer.Deserialize<CouponVO>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
