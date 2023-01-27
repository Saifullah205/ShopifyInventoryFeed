using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class MarkUpPrice
    {
        public int Id { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal MarkupPercentage { get; set; }
        public DateTime? AddDate { get; set; }
        public string? ApiType { get; set; }
        public int? EcomStoreId { get; set; }

        public virtual EcomStore? EcomStore { get; set; }
    }
}
