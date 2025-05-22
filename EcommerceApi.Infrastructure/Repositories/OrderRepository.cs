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
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            // Generate a unique order number based on date and random number
            string dateString = DateTime.UtcNow.ToString("yyyyMMdd");
            string randomString = new Random().Next(1000, 9999).ToString();

            string orderNumber = $"ORD-{dateString}-{randomString}";

            // Check if the order number already exists
            while (await _dbSet.AnyAsync(o => o.OrderNumber == orderNumber))
            {
                randomString = new Random().Next(1000, 9999).ToString();
                orderNumber = $"ORD-{dateString}-{randomString}";
            }

            return orderNumber;
        }

        public async Task<Order> GetOrderWithDetailsAsync(int orderId)
        {
            return await _dbSet
                .Include(o => o.User)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _dbSet.FindAsync(orderId);

            if (order == null)
                return false;

            order.Status = status;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
