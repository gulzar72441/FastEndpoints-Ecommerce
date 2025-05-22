using EcommerceApi.Api.Contracts.Cart;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace EcommerceApi.Api.Endpoints.Cart
{
    public class ApplyPromotionEndpoint : Endpoint<ApplyPromotionRequest, CartResponse>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly IPricingService _pricingService;

        public ApplyPromotionEndpoint(
            ICartRepository cartRepository,
            IPromotionRepository promotionRepository,
            IPricingService pricingService)
        {
            _cartRepository = cartRepository;
            _promotionRepository = promotionRepository;
            _pricingService = pricingService;
        }

        public override void Configure()
        {
            Post("/api/cart/apply-promotion");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Apply a promotion code to the cart";
                s.Description = "This endpoint applies a promotion code to the user's cart";
                s.Response<CartResponse>(StatusCodes.Status200OK, "Promotion applied successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid promotion code");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
            });
        }

        public override async Task HandleAsync(ApplyPromotionRequest req, CancellationToken ct)
        {
            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                ThrowError("User not authenticated properly", StatusCodes.Status401Unauthorized);
                return;
            }

            // Get user's cart
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            // Calculate subtotal
            decimal subtotal = cart.Items?.Sum(i => i.TotalPrice) ?? 0;

            // Validate promotion code
            var promotion = await _pricingService.ValidatePromotionCodeAsync(req.PromotionCode, subtotal);
            if (promotion == null)
            {
                AddError(r => r.PromotionCode, "Invalid or expired promotion code");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Apply promotion to get discounted total
            decimal discountedTotal = await _pricingService.ApplyPromotionAsync(subtotal, promotion);
            decimal discountAmount = subtotal - discountedTotal;

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
                AppliedPromotionCode = req.PromotionCode,
                DiscountAmount = discountAmount,
                Total = discountedTotal,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
