using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class ClientApi
    {
        public int Id { get; set; }
        public string? ApiType { get; set; }
        public string? ApiDescription { get; set; }
        public DateTime? DateCreated { get; set; }
    }
}
