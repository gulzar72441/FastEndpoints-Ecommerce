using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using EcommerceApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> AddItemToCartAsync(int cartId, int productId, int quantity)
        {
            try
            {
                // Check if the cart exists
                var cart = await _dbSet.FindAsync(cartId);
                if (cart == null)
                    return false;

                // Check if the product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null || !product.IsActive || product.StockQuantity < quantity)
                    return false;

                // Check if the item already exists in the cart
                var existingItem = await _context.CartItems
                    .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);

                if (existingItem != null)
                {
                    // Update existing item
                    existingItem.Quantity += quantity;
                    existingItem.TotalPrice = existingItem.UnitPrice * existingItem.Quantity;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new item
                    var cartItem = new CartItem
                    {
                        CartId = cartId,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = product.Price,
                        TotalPrice = product.Price * quantity,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.CartItems.AddAsync(cartItem);
                }

                // Update cart
                cart.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(int cartId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(i => i.CartId == cartId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);

                // Update cart
                var cart = await _dbSet.FindAsync(cartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CartItem> GetCartItemAsync(int cartId, int productId)
        {
            return await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
        }

        public async Task<Cart> GetCartWithItemsByUserIdAsync(int userId)
        {
            // Check if the user has a cart
            var cart = await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            // If not, create a new cart for the user
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };

                await _dbSet.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> RemoveItemFromCartAsync(int cartItemId)
        {
            try
            {
                var cartItem = await _context.CartItems.FindAsync(cartItemId);
                if (cartItem == null)
                    return false;

                _context.CartItems.Remove(cartItem);

                // Update cart
                var cart = await _dbSet.FindAsync(cartItem.CartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int cartItemId, int quantity)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(i => i.Product)
                    .FirstOrDefaultAsync(i => i.Id == cartItemId);

                if (cartItem == null)
                    return false;

                // Check if product has enough stock
                if (cartItem.Product.StockQuantity < quantity)
                    return false;

                if (quantity <= 0)
                {
                    // Remove item if quantity is 0 or negative
                    return await RemoveItemFromCartAsync(cartItemId);
                }

                // Update item
                cartItem.Quantity = quantity;
                cartItem.TotalPrice = cartItem.UnitPrice * quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;

                // Update cart
                var cart = await _dbSet.FindAsync(cartItem.CartId);
                if (cart != null)
                {
                    cart.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
