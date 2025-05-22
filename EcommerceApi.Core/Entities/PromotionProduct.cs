using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Entities
{
    public class PromotionProduct
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public virtual Promotion Promotion { get; set; }
        public virtual Product Product { get; set; }
    }
}
