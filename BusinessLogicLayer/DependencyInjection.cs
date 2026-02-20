using eShop.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eShop.OrdersMicroservice.BusinessLogicLayer.Services;
using eShop.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
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

    services.AddStackExchangeRedisCache(options =>
    {
      options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
    });

    services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();

    services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();

    services.AddHostedService<RabbitMQProductNameUpdateHostedService>();

    services.AddHostedService<RabbitMQProductDeletionHostedService>();

    return services;
  }
}