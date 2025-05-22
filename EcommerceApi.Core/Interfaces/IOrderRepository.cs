using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(int userId);
        Task<Order> GetOrderWithDetailsAsync(int orderId);
        Task<string> GenerateOrderNumberAsync();
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
    }
}
