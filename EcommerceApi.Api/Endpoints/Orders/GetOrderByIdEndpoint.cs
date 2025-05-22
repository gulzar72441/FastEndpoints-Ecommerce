using EcommerceApi.Api.Contracts.Orders;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Orders
{
    public class GetOrderByIdEndpoint : Endpoint<GetOrderByIdRequest, OrderResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUserRepository _userRepository;

        public GetOrderByIdEndpoint(IOrderRepository orderRepository, IUserRepository userRepository)
        {
            _orderRepository = orderRepository;
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Get("/api/orders/{Id}");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Get an order by ID";
                s.Description = "This endpoint returns an order by its ID";
                s.Response<OrderResponse>(StatusCodes.Status200OK, "Order retrieved successfully");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status403Forbidden, "Forbidden");
                s.Response(StatusCodes.Status404NotFound, "Order not found");
            });
        }

        public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
        {
            // Get order with details
            var order = await _orderRepository.GetOrderWithDetailsAsync(req.Id);
            if (order == null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                ThrowError("User not authenticated properly", StatusCodes.Status401Unauthorized);
                return;
            }

            // Check if user is authorized to view this order
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (order.UserId != userId && userRole != "Admin")
            {
                ThrowError("You are not authorized to view this order", StatusCodes.Status403Forbidden);
                return;
            }

            // Get user from database
            var user = await _userRepository.GetByIdAsync(order.UserId);
            if (user == null)
            {
                ThrowError("User not found", StatusCodes.Status404NotFound);
                return;
            }

            // Prepare response
            var response = new OrderResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                DeliveryDate = order.DeliveryDate,
                UserId = order.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                Items = order.Items?.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList(),
                Payment = order.Payment != null ? new PaymentResponse
                {
                    Id = order.Payment.Id,
                    PaymentMethod = order.Payment.PaymentMethod,
                    Amount = order.Payment.Amount,
                    Status = order.Payment.Status,
                    TransactionId = order.Payment.TransactionId,
                    PaymentDate = order.Payment.PaymentDate
                } : null
            };

            await SendAsync(response, cancellation: ct);
        }
    }

    public class GetOrderByIdRequest
    {
        public int Id { get; set; }
    }
}
