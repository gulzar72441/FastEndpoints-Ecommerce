using EcommerceApi.Api.Contracts.Cart;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace EcommerceApi.Api.Endpoints.Cart
{
    public class GetCartEndpoint : EndpointWithoutRequest<CartResponse>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPricingService _pricingService;

        public GetCartEndpoint(ICartRepository cartRepository, IPricingService pricingService)
        {
            _cartRepository = cartRepository;
            _pricingService = pricingService;
        }

        public override void Configure()
        {
            Get("/api/cart");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Get the current user's cart";
                s.Description = "This endpoint returns the current user's cart with all items";
                s.Response<CartResponse>(StatusCodes.Status200OK, "Cart retrieved successfully");
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

            // Get cart with items
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            // Calculate subtotal
            decimal subtotal = cart.Items?.Sum(i => i.TotalPrice) ?? 0;

            // Prepare response
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = cart.Items?.Select(i => new CartItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "Unknown Product",
                    ProductImageUrl = i.Product?.ImageUrl,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice
                }).ToList(),
                SubTotal = subtotal,
                AppliedPromotionCode = null,
                DiscountAmount = 0,
                Total = subtotal,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
