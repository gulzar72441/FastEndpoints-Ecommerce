using EcommerceApi.Core.Entities;
using EcommerceApi.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Infrastructure.Services
{
    public class PricingService : IPricingService
    {
        private readonly IProductRepository _productRepository;
        private readonly IPromotionRepository _promotionRepository;
        private readonly ICartRepository _cartRepository;

        public PricingService(
            IProductRepository productRepository,
            IPromotionRepository promotionRepository,
            ICartRepository cartRepository)
        {
            _productRepository = productRepository;
            _promotionRepository = promotionRepository;
            _cartRepository = cartRepository;
        }

        public async Task<decimal> ApplyPromotionAsync(decimal originalAmount, Promotion promotion)
        {
            if (promotion == null)
                return originalAmount;

            // Check if the order meets the minimum amount requirement
            if (promotion.MinimumOrderAmount.HasValue && originalAmount < promotion.MinimumOrderAmount.Value)
                return originalAmount;

            // Apply discount based on discount type
            if (promotion.DiscountType == "Percentage")
            {
                // Apply percentage discount
                var discountAmount = originalAmount * (promotion.DiscountValue / 100);
                return Math.Round(originalAmount - discountAmount, 2);
            }
            else if (promotion.DiscountType == "FixedAmount")
            {
                // Apply fixed amount discount
                return Math.Max(0, originalAmount - promotion.DiscountValue);
            }

            return originalAmount;
        }

        public async Task<decimal> CalculateCartTotalAsync(int cartId, string promotionCode = null)
        {
            // Get cart with items
            var cart = await _cartRepository.GetByIdAsync(cartId);
            if (cart == null)
                return 0;

            var cartItems = await _cartRepository.GetCartWithItemsByUserIdAsync(cart.UserId);
            if (cartItems.Items == null || !cartItems.Items.Any())
                return 0;

            // Calculate subtotal
            decimal subtotal = cartItems.Items.Sum(i => i.TotalPrice);

            // Apply promotion if provided
            if (!string.IsNullOrEmpty(promotionCode))
            {
                var promotion = await ValidatePromotionCodeAsync(promotionCode, subtotal);
                if (promotion != null)
                {
                    return await ApplyPromotionAsync(subtotal, promotion);
                }
            }

            return subtotal;
        }

        public async Task<decimal> CalculateOrderTotalAsync(List<OrderItem> orderItems, string promotionCode = null)
        {
            if (orderItems == null || !orderItems.Any())
                return 0;

            // Calculate subtotal
            decimal subtotal = orderItems.Sum(i => i.TotalPrice);

            // Apply promotion if provided
            if (!string.IsNullOrEmpty(promotionCode))
            {
                var promotion = await ValidatePromotionCodeAsync(promotionCode, subtotal);
                if (promotion != null)
                {
                    return await ApplyPromotionAsync(subtotal, promotion);
                }
            }

            return subtotal;
        }

        public async Task<decimal> CalculateProductPriceAsync(int productId, int quantity, string promotionCode = null)
        {
            // Get product
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
                return 0;

            // Calculate base price
            decimal basePrice = product.Price * quantity;

            // Check for product-specific promotions
            var productPromotions = await _promotionRepository.GetPromotionsForProductAsync(productId);

            // Find the best promotion (the one that gives the highest discount)
            decimal bestPrice = basePrice;
            Promotion bestPromotion = null;

            foreach (var promotion in productPromotions)
            {
                var discountedPrice = await ApplyPromotionAsync(basePrice, promotion);
                if (discountedPrice < bestPrice)
                {
                    bestPrice = discountedPrice;
                    bestPromotion = promotion;
                }
            }

            // Check if a promotion code was provided and compare with the best product promotion
            if (!string.IsNullOrEmpty(promotionCode))
            {
                var codePromotion = await ValidatePromotionCodeAsync(promotionCode, basePrice);
                if (codePromotion != null)
                {
                    var codeDiscountedPrice = await ApplyPromotionAsync(basePrice, codePromotion);
                    if (codeDiscountedPrice < bestPrice)
                    {
                        bestPrice = codeDiscountedPrice;
                    }
                }
            }

            return bestPrice;
        }

        public async Task<Promotion> ValidatePromotionCodeAsync(string code, decimal orderTotal)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            // Get promotion by code
            var promotion = await _promotionRepository.GetPromotionByCodeAsync(code);

            // Check if promotion exists and is active
            if (promotion == null || !promotion.IsActive)
                return null;

            // Check if promotion is within valid date range
            var now = DateTime.UtcNow;
            if (promotion.StartDate > now || promotion.EndDate < now)
                return null;

            // Check if order meets minimum amount requirement
            if (promotion.MinimumOrderAmount.HasValue && orderTotal < promotion.MinimumOrderAmount.Value)
                return null;

            return promotion;
        }
    }
}
