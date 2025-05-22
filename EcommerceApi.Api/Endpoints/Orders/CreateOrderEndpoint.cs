using EcommerceApi.Api.Contracts.Orders;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceApi.Api.Endpoints.Orders
{
    public class CreateOrderEndpoint : Endpoint<CreateOrderRequest, OrderResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentRepository _paymentRepository;

        public CreateOrderEndpoint(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUserRepository userRepository,
            IPaymentRepository paymentRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
            _paymentRepository = paymentRepository;
        }

        public override void Configure()
        {
            Post("/api/orders");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Create a new order";
                s.Description = "This endpoint creates a new order";
                s.Response<OrderResponse>(StatusCodes.Status201Created, "Order created successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
            });
        }

        public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
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

            // Validate order items
            if (req.Items == null || !req.Items.Any())
            {
                AddError(r => r.Items, "Order must contain at least one item");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Calculate order total and create order items
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var item in req.Items)
            {
                // Get product from database
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    AddError(r => r.Items, $"Product with ID {item.ProductId} not found");
                    continue;
                }

                // Check if product is active
                if (!product.IsActive)
                {
                    AddError(r => r.Items, $"Product {product.Name} is not available");
                    continue;
                }

                // Check if enough stock is available
                if (product.StockQuantity < item.Quantity)
                {
                    AddError(r => r.Items, $"Not enough stock for product {product.Name}. Available: {product.StockQuantity}");
                    continue;
                }

                // Calculate item total
                var itemTotal = product.Price * item.Quantity;
                totalAmount += itemTotal;

                // Create order item
                orderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = itemTotal
                });

                // Update product stock
                await _productRepository.UpdateStockAsync(product.Id, item.Quantity);
            }

            // If there are validation errors, return bad request
            if (ValidationFailed)
            {
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

            // Create new order
            var order = new EcommerceApi.Core.Entities.Order
            {
                OrderNumber = orderNumber,
                TotalAmount = totalAmount,
                Status = "Pending",
                ShippingAddress = req.ShippingAddress,
                OrderDate = DateTime.UtcNow,
                UserId = userId,
                Items = orderItems
            };

            // Save order to database
            await _orderRepository.AddAsync(order);

            // Create payment
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = req.Payment.PaymentMethod,
                Amount = totalAmount,
                Status = "Pending",
                TransactionId = Guid.NewGuid().ToString("N"),
                PaymentDate = DateTime.UtcNow
            };

            // Save payment to database
            await _paymentRepository.AddAsync(payment);

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
                Items = orderItems.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList(),
                Payment = new PaymentResponse
                {
                    Id = payment.Id,
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate
                }
            };

            await SendAsync(response, StatusCodes.Status201Created, ct);
        }
    }
}
