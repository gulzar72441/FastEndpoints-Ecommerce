using EcommerceApi.Api.Contracts.Cart;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace EcommerceApi.Api.Endpoints.Cart
{
    public class UpdateCartItemEndpoint : Endpoint<UpdateCartItemRequest, CartResponse>
    {
        private readonly ICartRepository _cartRepository;

        public UpdateCartItemEndpoint(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public override void Configure()
        {
            Put("/api/cart/items");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Update a cart item";
                s.Description = "This endpoint updates the quantity of an item in the user's cart";
                s.Response<CartResponse>(StatusCodes.Status200OK, "Cart item updated successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status404NotFound, "Cart item not found");
            });
        }

        public override async Task HandleAsync(UpdateCartItemRequest req, CancellationToken ct)
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

            // Check if the cart item exists and belongs to the user
            var cartItem = cart.Items?.FirstOrDefault(i => i.Id == req.CartItemId);
            if (cartItem == null)
            {
                ThrowError("Cart item not found", StatusCodes.Status404NotFound);
                return;
            }

            // Update cart item
            var success = await _cartRepository.UpdateCartItemAsync(req.CartItemId, req.Quantity);
            if (!success)
            {
                ThrowError("Failed to update cart item", StatusCodes.Status500InternalServerError);
                return;
            }

            // Get updated cart
            cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

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
