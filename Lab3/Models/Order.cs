using System;
using System.Collections.Generic;

namespace Lab3.Models
{
    public partial class Order
    {
        public int OrderId { get; set; }
        public int? BakeryProductId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public int? Quantity { get; set; }
        public decimal? Price { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public virtual BakeryProduct? BakeryProduct { get; set; }
    }
}
