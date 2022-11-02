using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync
{
    public class FragranceXProductsList
    {
        public FragranceXProductsList()
        {
            products = new List<FragranceXProduct>();
        }

        public List<FragranceXProduct> products { get; set; }
    }

    public class FragranceXProduct
    {
        public string ItemId { get; set; }
        public string BrandName { get; set; }
        public string ProductName { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public string MetricSize { get; set; }
        public string Upc { get; set; }
        public bool Instock { get; set; }
        public float RetailPriceUSD { get; set; }
        public float WholesalePriceUSD { get; set; }
        public float WholesalePriceGBP { get; set; }
        public float WholesalePriceEUR { get; set; }
        public float WholesalePriceAUD { get; set; }
        public float WholesalePriceCAD { get; set; }
        public string ParentCode { get; set; }
        public string Gender { get; set; }
        public string Description { get; set; }
        public string SmallImageUrl { get; set; }
        public string LargeImageUrl { get; set; }
        public int QuantityAvailable { get; set; }
    }


    public class FragranceXToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

}
