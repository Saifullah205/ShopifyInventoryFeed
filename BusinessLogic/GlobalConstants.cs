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
        public static string fragranceNetURL { get; set; }
        public static string fragranceNetUserName { get; set; }
        public static string fragranceNetPassword { get; set; }
        public static bool requiresShipping { get; set; }

        public enum STORENAME
        {
            SHOPIFY = 1,
            WALMART = 2
        }

        public enum APITYPE
        {
            FRAGRANCEX,
            TPS,
            FRAGRANCENET
        }
    }
}
