using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class ApplicationSetting
    {
        public int Id { get; set; }
        public string? Tag { get; set; }
        public string? TagValue { get; set; }
        public DateTime? AddDate { get; set; }
        public bool? IsVisible { get; set; }
    }
}
