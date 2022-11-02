using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{
    public static class GlobalConstants
    {
        public static string shopifySKUPrefix { get; set; }
        public static string minimumQuantity { get; set; }
        public static string markOutOfStock { get; set; }
        public static string locationId { get; set; }
        public static string shopifyAccessKey { get; set; }
        public static string shopifyBaseURL { get; set; }
        public static string fragranceXSKUPrefix { get; set; }
        public static string fragranceNetSKUPrefix { get; set; }
        public static string apiAccessId { get; set; }
        public static string apiAccessKey { get; set; }
        public static string grant_type { get; set; }
        public static string fragrancexURL { get; set; }
    }
}
