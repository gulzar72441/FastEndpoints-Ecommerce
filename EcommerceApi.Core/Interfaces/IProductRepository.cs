using EcommerceApi.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Interfaces
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IReadOnlyList<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IReadOnlyList<Product>> SearchProductsAsync(string searchTerm);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
