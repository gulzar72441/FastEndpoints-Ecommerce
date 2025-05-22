using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface IPricingService
    {
        Task<decimal> CalculateProductPriceAsync(int productId, int quantity, string promotionCode = null);
        Task<decimal> CalculateCartTotalAsync(int cartId, string promotionCode = null);
        Task<decimal> CalculateOrderTotalAsync(List<OrderItem> orderItems, string promotionCode = null);
        Task<Promotion> ValidatePromotionCodeAsync(string code, decimal orderTotal);
        Task<decimal> ApplyPromotionAsync(decimal originalAmount, Promotion promotion);
    }
}
