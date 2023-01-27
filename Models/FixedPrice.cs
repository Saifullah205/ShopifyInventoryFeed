using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class FixedPrice
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? FixPrice { get; set; }
        public DateTime? DateCreated { get; set; }
        public string? ApiType { get; set; }
        public int? EcomStoreId { get; set; }

        public virtual EcomStore? EcomStore { get; set; }
    }
}
