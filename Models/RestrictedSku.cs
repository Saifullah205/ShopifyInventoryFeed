using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class RestrictedSku
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public DateTime? AddDate { get; set; }
        public string? ApiType { get; set; }
    }
}
