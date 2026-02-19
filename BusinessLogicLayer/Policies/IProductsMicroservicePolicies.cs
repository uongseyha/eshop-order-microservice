using Polly;

namespace eShop.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductsMicroservicePolicies
{
  IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy();
  IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy();
}
