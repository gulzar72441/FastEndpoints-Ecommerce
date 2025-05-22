using EcommerceApi.Api.Contracts.Products;

namespace EcommerceApi.Api.Contracts.Categories
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductResponse> Products { get; set; }
    }
}
