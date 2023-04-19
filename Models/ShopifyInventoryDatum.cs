using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class ShopifyInventoryDatum
    {
        public int Id { get; set; }
        public string? ShopifyId { get; set; }
        public string? Sku { get; set; }
        public DateTime? AddDate { get; set; }
        public string? InventoryItemId { get; set; }
        public string? BrandName { get; set; }
        public string? SkuPrefix { get; set; }
        public bool? IsRestricted { get; set; }
        public bool? IsOutOfStock { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsDisabled { get; set; }
        public string? VariantId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductGender { get; set; }
        public string? ImageId { get; set; }
        public decimal? Price { get; set; }
    }
}
