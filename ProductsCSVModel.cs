using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync
{
    public class ThePerfumeSpotProduct
    {
        [Index(0)]
        public string SKU { get; set; }
        [Index(1)]
        public string Name { get; set; }
        [Index(2)]
        public string Women { get; set; }
        [Index(3)]
        public string Men { get; set; }
        [Index(4)]
        public string GiftSet { get; set; }
        [Index(5)]
        public string Retail { get; set; }
        [Index(6)]
        public string YourCost { get; set; }
        [Index(7)]
        public string Brand { get; set; }
        [Index(8)]
        public string UPC { get; set; }
        [Index(9)]
        public string Weight { get; set; }
        [Index(10)]
        public string Inventory { get; set; }
        [Index(11)]
        public string ImageURL { get; set; }
    }

    public class ThePerfumeSpotProductsList
    {
        public ThePerfumeSpotProductsList()
        {
            products = new List<ThePerfumeSpotProduct>();
        }

        public List<ThePerfumeSpotProduct> products { get; set; }
    }
}
