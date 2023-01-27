using System;
using System.Collections.Generic;

namespace ShopifyInventorySync.Models
{
    public partial class EcomStore
    {
        public EcomStore()
        {
            FixedPrices = new HashSet<FixedPrice>();
            MarkUpPrices = new HashSet<MarkUpPrice>();
            RestrictedBrands = new HashSet<RestrictedBrand>();
            RestrictedSkus = new HashSet<RestrictedSku>();
        }

        public int Id { get; set; }
        public string? StoreName { get; set; }
        public DateTime? DateCreated { get; set; }

        public virtual ICollection<FixedPrice> FixedPrices { get; set; }
        public virtual ICollection<MarkUpPrice> MarkUpPrices { get; set; }
        public virtual ICollection<RestrictedBrand> RestrictedBrands { get; set; }
        public virtual ICollection<RestrictedSku> RestrictedSkus { get; set; }
    }
}
