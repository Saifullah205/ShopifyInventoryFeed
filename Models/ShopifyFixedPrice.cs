using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class ShopifyFixedPrice
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? FixedPrice { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
