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
    public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
    {
        public PromotionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<bool> AddCategoryToPromotionAsync(int promotionId, int categoryId)
        {
            try
            {
                // Check if the promotion exists
                var promotion = await _dbSet.FindAsync(promotionId);
                if (promotion == null)
                    return false;

                // Check if the category exists
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                    return false;

                // Check if the relationship already exists
                var exists = await _context.PromotionCategories
                    .AnyAsync(pc => pc.PromotionId == promotionId && pc.CategoryId == categoryId);

                if (exists)
                    return true;

                // Add the relationship
                var promotionCategory = new PromotionCategory
                {
                    PromotionId = promotionId,
                    CategoryId = categoryId
                };

                await _context.PromotionCategories.AddAsync(promotionCategory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddProductToPromotionAsync(int promotionId, int productId)
        {
            try
            {
                // Check if the promotion exists
                var promotion = await _dbSet.FindAsync(promotionId);
                if (promotion == null)
                    return false;

                // Check if the product exists
                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                    return false;

                // Check if the relationship already exists
                var exists = await _context.PromotionProducts
                    .AnyAsync(pp => pp.PromotionId == promotionId && pp.ProductId == productId);

                if (exists)
                    return true;

                // Add the relationship
                var promotionProduct = new PromotionProduct
                {
                    PromotionId = promotionId,
                    ProductId = productId
                };

                await _context.PromotionProducts.AddAsync(promotionProduct);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .ToListAsync();
        }

        public async Task<Promotion> GetPromotionByCodeAsync(string code)
        {
            return await _dbSet
                .Include(p => p.PromotionProducts)
                    .ThenInclude(pp => pp.Product)
                .Include(p => p.PromotionCategories)
                    .ThenInclude(pc => pc.Category)
                .FirstOrDefaultAsync(p => p.Code.ToLower() == code.ToLower());
        }

        public async Task<IReadOnlyList<Promotion>> GetPromotionsForCategoryAsync(int categoryId)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .Where(p => p.PromotionCategories.Any(pc => pc.CategoryId == categoryId))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Promotion>> GetPromotionsForProductAsync(int productId)
        {
            var now = DateTime.UtcNow;

            // Get promotions directly associated with the product
            var productPromotions = await _dbSet
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .Where(p => p.PromotionProducts.Any(pp => pp.ProductId == productId))
                .ToListAsync();

            // Get the product's category ID
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return productPromotions;

            // Get promotions associated with the product's category
            var categoryPromotions = await _dbSet
                .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now)
                .Where(p => p.PromotionCategories.Any(pc => pc.CategoryId == product.CategoryId))
                .ToListAsync();

            // Combine and return unique promotions
            return productPromotions
                .Union(categoryPromotions)
                .ToList();
        }

        public async Task<bool> RemoveCategoryFromPromotionAsync(int promotionId, int categoryId)
        {
            try
            {
                var promotionCategory = await _context.PromotionCategories
                    .FirstOrDefaultAsync(pc => pc.PromotionId == promotionId && pc.CategoryId == categoryId);

                if (promotionCategory == null)
                    return false;

                _context.PromotionCategories.Remove(promotionCategory);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveProductFromPromotionAsync(int promotionId, int productId)
        {
            try
            {
                var promotionProduct = await _context.PromotionProducts
                    .FirstOrDefaultAsync(pp => pp.PromotionId == promotionId && pp.ProductId == productId);

                if (promotionProduct == null)
                    return false;

                _context.PromotionProducts.Remove(promotionProduct);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearPromotionProductsAsync(int promotionId)
        {
            try
            {
                // Check if the promotion exists
                var promotion = await _dbSet.FindAsync(promotionId);
                if (promotion == null)
                    return false;

                // Get all product associations for this promotion
                var promotionProducts = await _context.PromotionProducts
                    .Where(pp => pp.PromotionId == promotionId)
                    .ToListAsync();

                // Remove all associations
                _context.PromotionProducts.RemoveRange(promotionProducts);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearPromotionCategoriesAsync(int promotionId)
        {
            try
            {
                // Check if the promotion exists
                var promotion = await _dbSet.FindAsync(promotionId);
                if (promotion == null)
                    return false;

                // Get all category associations for this promotion
                var promotionCategories = await _context.PromotionCategories
                    .Where(pc => pc.PromotionId == promotionId)
                    .ToListAsync();

                // Remove all associations
                _context.PromotionCategories.RemoveRange(promotionCategories);
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
