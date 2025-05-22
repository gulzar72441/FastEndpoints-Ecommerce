using EcommerceApi.Api.Contracts.Cart;
using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using FastEndpoints;
using System.Security.Claims;

namespace EcommerceApi.Api.Endpoints.Cart
{
    public class AddToCartEndpoint : Endpoint<AddToCartRequest, CartResponse>
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public AddToCartEndpoint(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public override void Configure()
        {
            Post("/api/cart/items");
            Roles("Customer", "Admin");
            Summary(s =>
            {
                s.Summary = "Add an item to the cart";
                s.Description = "This endpoint adds a product to the user's cart";
                s.Response<CartResponse>(StatusCodes.Status200OK, "Item added to cart successfully");
                s.Response(StatusCodes.Status400BadRequest, "Invalid request");
                s.Response(StatusCodes.Status401Unauthorized, "Unauthorized");
                s.Response(StatusCodes.Status404NotFound, "Product not found");
            });
        }

        public override async Task HandleAsync(AddToCartRequest req, CancellationToken ct)
        {
            // Validate request
            if (req.Quantity <= 0)
            {
                AddError(r => r.Quantity, "Quantity must be greater than 0");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Get current user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                ThrowError("User not authenticated properly", StatusCodes.Status401Unauthorized);
                return;
            }

            // Check if product exists
            var product = await _productRepository.GetByIdAsync(req.ProductId);
            if (product == null)
            {
                ThrowError("Product not found", StatusCodes.Status404NotFound);
                return;
            }

            // Check if product is active
            if (!product.IsActive)
            {
                AddError(r => r.ProductId, "Product is not available");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Check if enough stock is available
            if (product.StockQuantity < req.Quantity)
            {
                AddError(r => r.Quantity, $"Not enough stock. Available: {product.StockQuantity}");
                await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
                return;
            }

            // Get user's cart
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId);

            // Add item to cart
            var success = await _cartRepository.AddItemToCartAsync(cart.Id, req.ProductId, req.Quantity);
            if (!success)
            {
                ThrowError("Failed to add item to cart", StatusCodes.Status500InternalServerError);
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
