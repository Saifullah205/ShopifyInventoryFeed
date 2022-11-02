using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{
    public class FragranceNetProduct
    {
        [Index(0)]
        public string sku { get; set; }
        [Index(1)]
        public string productCategory { get; set; }
        [Index(2)]
        public string itemType { get; set; }
        [Index(3)]
        public string name { get; set; }
        [Index(4)]
        public string designer { get; set; }
        [Index(5)]
        public string brand { get; set; }
        [Index(6)]
        public string productDescription { get; set; }
        [Index(7)]
        public string gender { get; set; }
        [Index(8)]
        public string fragranceNotes { get; set; }
        [Index(9)]
        public string yearIntroduced { get; set; }
        [Index(10)]
        public string recommendedUse { get; set; }
        [Index(11)]
        public string msrp { get; set; }
        [Index(12)]
        public string fnetWholesalePrice { get; set; }
        [Index(13)]
        public string imageLarge { get; set; }
        [Index(14)]
        public string imageSmall { get; set; }
        [Index(15)]
        public string url { get; set; }
    }

    public class FragranceNetProductsList
    {
        public FragranceNetProductsList()
        {
            products = new List<FragranceNetProduct>();
        }

        public List<FragranceNetProduct> products { get; set; }
    }
}
