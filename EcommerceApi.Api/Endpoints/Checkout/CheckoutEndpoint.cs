using EcommerceApi.Api.Contracts.Checkout;
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

namespace EcommerceApi.Api.Endpoints.Checkout
{
    public class CheckoutEndpoint : Endpoint<CheckoutRequest, CheckoutResponse>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;
        private readonly IPricingService _pricingService;

        public CheckoutEndpoint(
            ICartRepository cartRepository,
            IOrderRepository orderRepository,
            IPaymentRepository paymentRepository,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IPricingService pricingService)
        {
            _cartRepository = cartRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _userRepository = userRepository;
            _productRepository = productRepository;
            _pricingService = pricingService;
        }

        public override void Configure()
        {
            Post("/api/checkout");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Process checkout";
                s.Description = "This endpoint processes the checkout from the user's cart";
                s.Response<CheckoutResponse>(StatusCodes.Status200OK, "Checkout successful");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status404NotFound, "Cart not found");
            });
        }

        public override async Task HandleAsync(CheckoutRequest req, CancellationToken ct)
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                ThrowError("User not authenticated properly", StatusCodes.Status401Unauthorized);
                return;
            }

            // Get user
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                ThrowError("User not found", StatusCodes.Status404NotFound);
                return;
            }

            // Get user's cart
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);
            if (cart == null || cart.Items == null || !cart.Items.Any())
            {
                ThrowError("Cart is empty", StatusCodes.Status400BadRequest);
                return;
            }

            // Calculate subtotal
            decimal subtotal = cart.Items.Sum(i => i.TotalPrice);

            // Apply promotion if provided
            decimal discountAmount = 0;
            decimal total = subtotal;
            var promotion = !string.IsNullOrEmpty(req.PromotionCode)
                ? await _pricingService.ValidatePromotionCodeAsync(req.PromotionCode, subtotal)
                : null;

            if (promotion != null)
            {
                total = await _pricingService.ApplyPromotionAsync(subtotal, promotion);
                discountAmount = subtotal - total;
            }

            // Generate order number
            var orderNumber = await _orderRepository.GenerateOrderNumberAsync();

            // Create order items
            var orderItems = new List<OrderItem>();
            foreach (var cartItem in cart.Items)
            {
                // Get product
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null || !product.IsActive || product.StockQuantity < cartItem.Quantity)
                {
                    AddError("Items", $"Product {cartItem.Product?.Name ?? "Unknown"} is no longer available or has insufficient stock");
                    continue;
                }

                // Create order item
                orderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice,
                    TotalPrice = cartItem.TotalPrice
                });

                // Update product stock
                await _productRepository.UpdateStockAsync(cartItem.ProductId, cartItem.Quantity);
            }

            // If there are validation errors, return bad request
            if (ValidationFailed)
            {
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Create order
            var order = new EcommerceApi.Core.Entities.Order
            {
                OrderNumber = orderNumber,
                TotalAmount = total,
                Status = "Pending",
                ShippingAddress = req.ShippingAddress,
                OrderDate = DateTime.UtcNow,
                UserId = userId,
                Items = orderItems
            };

            // Save order
            await _orderRepository.AddAsync(order);

            // Create payment
            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = req.Payment.PaymentMethod,
                Amount = total,
                Status = "Pending",
                TransactionId = Guid.NewGuid().ToString("N"),
                PaymentDate = DateTime.UtcNow
            };

            // Save payment
            await _paymentRepository.AddAsync(payment);

            // Clear cart
            await _cartRepository.ClearCartAsync(cart.Id);

            // Prepare response
            var response = new CheckoutResponse
            {
                Success = true,
                Message = "Checkout successful",
                Order = new OrderResponse
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
                },
                Payment = new PaymentResponse
                {
                    Id = payment.Id,
                    PaymentMethod = payment.PaymentMethod,
                    Amount = payment.Amount,
                    Status = payment.Status,
                    TransactionId = payment.TransactionId,
                    PaymentDate = payment.PaymentDate
                },
                SubTotal = subtotal,
                DiscountAmount = discountAmount,
                Total = total,
                PromotionApplied = promotion?.Code,
                CheckoutDate = DateTime.UtcNow
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
