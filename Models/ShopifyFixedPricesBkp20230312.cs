using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class ShopifyFixedPricesBkp20230312
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? FixedPrice { get; set; }
        public DateTime? AddDate { get; set; }
        public string? ApiType { get; set; }
    }
}
