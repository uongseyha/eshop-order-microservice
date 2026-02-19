using eShop.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient(
  HttpClient _httpClient
  , ILogger<UsersMicroserviceClient> _logger
  , IDistributedCache _distributedCache)
{
  public async Task<UserDTO?> GetUserByUserID(Guid userID)
  {
    try
    {
      string cacheKeyToRead = $"user:{userID}";
      string? cachedUser = await _distributedCache.GetStringAsync(cacheKeyToRead);

      if (cachedUser != null)
      {
        //Deserialized the cached user
        UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO>(cachedUser);

        return userFromCache;
      }

      HttpResponseMessage response = await _httpClient.GetAsync($"/api/users/{userID}");

      if (!response.IsSuccessStatusCode)
      {
        if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
        {
          UserDTO? fallbackUser = await response.Content.ReadFromJsonAsync<UserDTO>();

          if (fallbackUser == null)
          {
            throw new NotImplementedException();
          }

          return fallbackUser;
        }

        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
          return null;
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
          throw new HttpRequestException("Bad request", null, System.Net.HttpStatusCode.BadRequest);
        }
        else
        {
          //throw new HttpRequestException($"Http request failed with status code {response.StatusCode}");

          return new UserDTO(
            PersonName: "Temporarily Unavailable",
            Email: "Temporarily Unavailable",
            Gender: "Temporarily Unavailable",
            UserID: Guid.Empty);
        }
      }


      UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();

      if (user == null)
      {
        throw new ArgumentException("Invalid User ID");
      }

      //Store the user data (retrieved from response) into cache
      string cacheKey = $"user:{userID}";
      string userJson = JsonSerializer.Serialize(user);

      DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddMinutes(5))
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(3));

      await _distributedCache.SetStringAsync(cacheKey, userJson, options);

      return user;
    }
    catch (BrokenCircuitException ex)
    {
      _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");

      return new UserDTO(
              PersonName: "Temporarily Unavailable (circuit breaker)",
              Email: "Temporarily Unavailable (circuit breaker)",
              Gender: "Temporarily Unavailable (circuit breaker)",
              UserID: Guid.Empty);
    }

    catch (TimeoutRejectedException ex)
    {
      _logger.LogError(ex, "Timeout occurred while fetching user data. Returning dummy data");

      return new UserDTO(
              PersonName: "Temporarily Unavailable (timeout)",
              Email: "Temporarily Unavailable (timeout)",
              Gender: "Temporarily Unavailable (timeout)",
              UserID: Guid.Empty);
    }
  }
}

