using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceApi.Core.Entities
{
    public class PromotionCategory
    {
        public int Id { get; set; }
        public int PromotionId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual Promotion Promotion { get; set; }
        public virtual Category Category { get; set; }
    }
}
