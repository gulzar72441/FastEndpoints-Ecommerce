namespace EcommerceApi.Api.Contracts.Promotions
{
    public class PromotionResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<PromotionProductResponse> Products { get; set; }
        public List<PromotionCategoryResponse> Categories { get; set; }
    }

    public class PromotionProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }

    public class PromotionCategoryResponse
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}
