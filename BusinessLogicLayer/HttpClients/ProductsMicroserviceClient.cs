using eShop.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Net.Http.Json;

namespace eShop.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient(HttpClient _httpClient, ILogger<ProductsMicroserviceClient> _logger)
{
  public async Task<ProductDTO?> GetProductByProductID(Guid productID)
  {
    try
    {
      HttpResponseMessage response = await _httpClient.GetAsync($"/api/products/search/product-id/{productID}");

      if (!response.IsSuccessStatusCode)
      {
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          return null;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
          throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
        }
        else
        {
          throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");
        }
      }


      ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();

      if (product == null)
      {
        throw new ArgumentException("Invalid Product ID");
      }

      return product;
    }
    catch (BulkheadRejectedException ex)
    {
      _logger.LogError(ex, "Bulkhead isolation blocks the request since the request queue is full");

      return new ProductDTO(
        ProductID: Guid.NewGuid(),
        ProductName: "Temporarily Unavailable (Bulkhead)",
        Category: "Temporarily Unavailable (Bulkhead)",
        UnitPrice: 0,
        QuantityInStock: 0);
    }
  }
}

