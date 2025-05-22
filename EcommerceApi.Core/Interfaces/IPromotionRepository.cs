using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface IPromotionRepository : IGenericRepository<Promotion>
    {
        Task<Promotion> GetPromotionByCodeAsync(string code);
        Task<IReadOnlyList<Promotion>> GetActivePromotionsAsync();
        Task<IReadOnlyList<Promotion>> GetPromotionsForProductAsync(int productId);
        Task<IReadOnlyList<Promotion>> GetPromotionsForCategoryAsync(int categoryId);
        Task<bool> AddProductToPromotionAsync(int promotionId, int productId);
        Task<bool> AddCategoryToPromotionAsync(int promotionId, int categoryId);
        Task<bool> RemoveProductFromPromotionAsync(int promotionId, int productId);
        Task<bool> RemoveCategoryFromPromotionAsync(int promotionId, int categoryId);
        Task<bool> ClearPromotionProductsAsync(int promotionId);
        Task<bool> ClearPromotionCategoriesAsync(int promotionId);
    }
}
