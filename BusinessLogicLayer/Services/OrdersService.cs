using AutoMapper;
using eShop.OrdersMicroservice.BusinessLogicLayer.DTO;
using eShop.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eShop.OrdersMicroservice.DataAccessLayer.Entities;
using eShop.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;
using MongoDB.Driver;

namespace eShop.ordersMicroservice.BusinessLogicLayer.Services;

public class OrdersService(
  IOrdersRepository ordersRepository,
  IMapper mapper,
  IValidator<OrderAddRequest> orderAddRequestValidator,
  IValidator<OrderItemAddRequest> orderItemAddRequestValidator,
  IValidator<OrderUpdateRequest> orderUpdateRequestValidator,
  IValidator<OrderItemUpdateRequest> orderItemUpdateRequestValidator) : IOrdersService
{
  public async Task<OrderResponse?> AddOrder(OrderAddRequest orderAddRequest)
  {
    //Check for null parameter
    if (orderAddRequest == null)
    {
      throw new ArgumentNullException(nameof(orderAddRequest));
    }


    //Validate OrderAddRequest using Fluent Validations
    ValidationResult orderAddRequestValidationResult = await orderAddRequestValidator.ValidateAsync(orderAddRequest);
    if (!orderAddRequestValidationResult.IsValid)
    {
      string errors = string.Join(", ", orderAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
      throw new ArgumentException(errors);
    }

    //Validate order items using Fluent Validation
    foreach (OrderItemAddRequest orderItemAddRequest in orderAddRequest.OrderItems)
    {
      ValidationResult orderItemAddRequestValidationResult = await orderItemAddRequestValidator.ValidateAsync(orderItemAddRequest);

      if (!orderItemAddRequestValidationResult.IsValid)
      {
        string errors = string.Join(", ", orderItemAddRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
        throw new ArgumentException(errors);
      }
    }

    //TO DO: Add logic for checking if UserID exists in Users microservice


    //Convert data from OrderAddRequest to Order
    Order orderInput = mapper.Map<Order>(orderAddRequest); //Map OrderAddRequest to 'Order' type (it invokes OrderAddRequestToOrderMappingProfile class)

    //Generate values
    foreach (OrderItem orderItem in orderInput.OrderItems) 
    {
      orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
    }
    orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);


    //Invoke repository
    Order? addedOrder = await ordersRepository.AddOrder(orderInput);

    if (addedOrder == null) 
    {
      return null;
    }

    OrderResponse addedOrderResponse = mapper.Map<OrderResponse>(addedOrder); //Map addedOrder ('Order' type) into 'OrderResponse' type (it invokes OrderToOrderResponseMappingProfile).

    return addedOrderResponse;
  }



  public async Task<OrderResponse?> UpdateOrder(OrderUpdateRequest orderUpdateRequest)
  {
    //Check for null parameter
    if (orderUpdateRequest == null)
    {
      throw new ArgumentNullException(nameof(orderUpdateRequest));
    }


    //Validate OrderAddRequest using Fluent Validations
    ValidationResult orderUpdateRequestValidationResult = await orderUpdateRequestValidator.ValidateAsync(orderUpdateRequest);
    if (!orderUpdateRequestValidationResult.IsValid)
    {
      string errors = string.Join(", ", orderUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
      throw new ArgumentException(errors);
    }

    //Validate order items using Fluent Validation
    foreach (OrderItemUpdateRequest orderItemUpdateRequest in orderUpdateRequest.OrderItems)
    {
      ValidationResult orderItemUpdateRequestValidationResult = await orderItemUpdateRequestValidator.ValidateAsync(orderItemUpdateRequest);

      if (!orderItemUpdateRequestValidationResult.IsValid)
      {
        string errors = string.Join(", ", orderItemUpdateRequestValidationResult.Errors.Select(temp => temp.ErrorMessage));
        throw new ArgumentException(errors);
      }
    }

    //TO DO: Add logic for checking if UserID exists in Users microservice


    //Convert data from OrderUpdateRequest to Order
    Order orderInput = mapper.Map<Order>(orderUpdateRequest); //Map OrderUpdateRequest to 'Order' type (it invokes OrderUpdateRequestToOrderMappingProfile class)

    //Generate values
    foreach (OrderItem orderItem in orderInput.OrderItems)
    {
      orderItem.TotalPrice = orderItem.Quantity * orderItem.UnitPrice;
    }
    orderInput.TotalBill = orderInput.OrderItems.Sum(temp => temp.TotalPrice);


    //Invoke repository
    Order? updatedOrder = await ordersRepository.UpdateOrder(orderInput);

    if (updatedOrder == null)
    {
      return null;
    }

    OrderResponse updatedOrderResponse = mapper.Map<OrderResponse>(updatedOrder); //Map updatedOrder ('Order' type) into 'OrderResponse' type (it invokes OrderToOrderResponseMappingProfile).

    return updatedOrderResponse;
  }


  public async Task<bool> DeleteOrder(Guid orderID)
  {
    FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
    Order? existingOrder = await ordersRepository.GetOrderByCondition(filter);

    if (existingOrder == null)
    {
      return false;
    }


    bool isDeleted = await ordersRepository.DeleteOrder(orderID);
    return isDeleted;
  }


  public async Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter)
  {
    Order? order = await ordersRepository.GetOrderByCondition(filter);
    if (order == null)
      return null;

    OrderResponse orderResponse = mapper.Map<OrderResponse>(order);
    return orderResponse;
  }


  public async Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order> filter)
  {
    IEnumerable<Order?> orders = await ordersRepository.GetOrdersByCondition(filter);


    IEnumerable<OrderResponse?> orderResponses = mapper.Map<IEnumerable<OrderResponse>>(orders);
    return orderResponses.ToList();
  }


  public async Task<List<OrderResponse?>> GetOrders()
  {
    IEnumerable<Order?> orders = await ordersRepository.GetOrders();


    IEnumerable<OrderResponse?> orderResponses = mapper.Map<IEnumerable<OrderResponse>>(orders);
    return orderResponses.ToList();
  }
}