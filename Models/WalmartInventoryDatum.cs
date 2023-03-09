using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class WalmartInventoryDatum
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public DateTime? AddDate { get; set; }
        public string? BrandName { get; set; }
        public string? SkuPrefix { get; set; }
        public bool? IsShippingMapped { get; set; }
    }
}
