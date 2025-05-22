using EcommerceApi.Api.Contracts.Cart;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace EcommerceApi.Api.Endpoints.Cart
{
    public class ClearCartEndpoint : EndpointWithoutRequest<CartResponse>
    {
        private readonly ICartRepository _cartRepository;

        public ClearCartEndpoint(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public override void Configure()
        {
            Delete("/api/cart");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Clear the cart";
                s.Description = "This endpoint removes all items from the user's cart";
                s.Response<CartResponse>(StatusCodes.Status200OK, "Cart cleared successfully");
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

            // Get user's cart
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            // Clear cart
            var success = await _cartRepository.ClearCartAsync(cart.Id);
            if (!success)
            {
                ThrowError("Failed to clear cart", StatusCodes.Status500InternalServerError);
                return;
            }

            // Get updated cart
            cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            // Prepare response
            var response = new CartResponse
            {
                Id = cart.Id,
                UserId = cart.UserId,
                Items = new System.Collections.Generic.List<CartItemResponse>(),
                SubTotal = 0,
                AppliedPromotionCode = null,
                DiscountAmount = 0,
                Total = 0,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt
            };

            await SendAsync(response, cancellation: ct);
        }
    }
}
