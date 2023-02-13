using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class WalmartFeedResponse
    {
        public int Id { get; set; }
        public string? FeedId { get; set; }
        public string? Status { get; set; }
        public DateTime? AddDate { get; set; }
    }
}
