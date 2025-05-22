using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart> GetCartWithItemsByUserIdAsync(int userId);
        Task<CartItem> GetCartItemAsync(int cartId, int productId);
        Task<bool> AddItemToCartAsync(int cartId, int productId, int quantity);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> RemoveItemFromCartAsync(int cartItemId);
        Task<bool> ClearCartAsync(int cartId);
    }
}
