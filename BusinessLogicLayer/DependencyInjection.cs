using eShop.ordersMicroservice.BusinessLogicLayer.Mappers;
using eShop.ordersMicroservice.BusinessLogicLayer.Services;
using eShop.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eShop.OrdersMicroservice.BusinessLogicLayer.Validators;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace eShop.OrderMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
  public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
  {
    //TO DO: Add business logic layer services into the IoC container
    services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();
    services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);
    services.AddScoped<IOrdersService, OrdersService>();
    return services;
  }
}