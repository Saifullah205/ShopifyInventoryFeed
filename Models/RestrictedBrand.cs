using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class RestrictedBrand
    {
        public int Id { get; set; }
        public string? BrandName { get; set; }
        public DateTime? AddDate { get; set; }
        public string? ApiType { get; set; }
        public int? EcomStoreId { get; set; }

        public virtual EcomStore? EcomStore { get; set; }
    }
}
