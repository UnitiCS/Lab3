using System;
using System.Collections.Generic;

namespace Lab3.Models
{
    public partial class BakeryProduct
    {
        public BakeryProduct()
        {
            Orders = new HashSet<Order>();
        }

        public int BakeryProductId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
