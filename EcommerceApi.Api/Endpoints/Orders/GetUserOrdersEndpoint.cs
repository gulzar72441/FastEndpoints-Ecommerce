using EcommerceApi.Api.Contracts.Orders;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Orders
{
    public class GetUserOrdersEndpoint : EndpointWithoutRequest<List<OrderResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public GetUserOrdersEndpoint(IOrderRepository orderRepository, IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Get("/api/orders/my-orders");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Get all orders for the current user";
                s.Description = "This endpoint returns all orders for the authenticated user";
                s.Response<List<OrderResponse>>(StatusCodes.Status200OK, "Orders retrieved successfully");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                ThrowError("User not authenticated properly", StatusCodes.Status401Unauthorized);
                return;
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                ThrowError("User not found", StatusCodes.Status404NotFound);
                return;
            }

            // Get orders for the user
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);

            // Prepare response
            var response = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ShippingAddress = o.ShippingAddress,
                OrderDate = o.OrderDate,
                DeliveryDate = o.DeliveryDate,
                UserId = o.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                Items = o.Items?.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList(),
                Payment = o.Payment != null ? new PaymentResponse
                {
                    Id = o.Payment.Id,
                    PaymentMethod = o.Payment.PaymentMethod,
                    Amount = o.Payment.Amount,
                    Status = o.Payment.Status,
                    TransactionId = o.Payment.TransactionId,
                    PaymentDate = o.Payment.PaymentDate
                } : null
            }).ToList();

            await SendAsync(response, cancellation: ct);
        }
    }
}
