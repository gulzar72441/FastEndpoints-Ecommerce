using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Api.Contracts.Promotions
{
    public class CreatePromotionRequest
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string DiscountType { get; set; } // Percentage, FixedAmount

        [Required]
        public decimal DiscountValue { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public List<int> ProductIds { get; set; } = new List<int>();

        public List<int> CategoryIds { get; set; } = new List<int>();
    }
}
