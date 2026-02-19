using Polly;

namespace eShop.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IUsersMicroservicePolicies
{
  IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy();
}
