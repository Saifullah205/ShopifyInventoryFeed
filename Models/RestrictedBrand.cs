using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class RestrictedBrand
    {
        public int Id { get; set; }
        public string? BrandName { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
