﻿using GeekShopping.Web.Models;
using GeekShopping.Web.Services.IServices;
using GeekShopping.Web.Utils;

namespace GeekShopping.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        public const string BasePath = "api/v1/product";

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IEnumerable<ProductModel>> FindAllProducts()
        {
            var response = await _httpClient.GetAsync(BasePath);
            return await response.ReadContentAs<List<ProductModel>>();
        }

        public async Task<ProductModel> FindProductById(long id)
        {
            var response = await _httpClient.GetAsync($"{BasePath}/{id}");
            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<ProductModel> CreateProduct(ProductModel productModel)
        {
            var response = await _httpClient.PostAsJson(BasePath, productModel);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something went wrong calling API");

            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<ProductModel> UpdateProduct(ProductModel productModel)
        {
            var response = await _httpClient.PutAsJson(BasePath, productModel);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something wrong calling API");

            return await response.ReadContentAs<ProductModel>();
        }

        public async Task<bool> DeleteProductById(long id)
        {
            var response = await _httpClient.DeleteAsync($"{BasePath}/{id}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Something wrong calling API");

            return await response.ReadContentAs<bool>();
        }
    }
}
