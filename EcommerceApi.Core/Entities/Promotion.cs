using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Entities
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public string DiscountType { get; set; } // Percentage, FixedAmount
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PromotionProduct> PromotionProducts { get; set; }
        public virtual ICollection<PromotionCategory> PromotionCategories { get; set; }
    }
}
