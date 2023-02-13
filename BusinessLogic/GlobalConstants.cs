namespace ShopifyInventorySync.BusinessLogic
{
    public static class GlobalConstants
    {
        public static string tpsSKUPrefix { get; set; }
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
        public static string wmConsumerChannelType { get; set; }
        public static string wmQosCorrelationId { get; set; }
        public static string wmSvcName { get; set; }
        public static string walmartAuthorization { get; set; }
        public static string walmartURL { get; set; }

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

        public enum LOGTYPE
        {
            ERROR = 1,
            INFO = 2
        }

        public enum WALMARTFEEDTYPE
        {
            MP_ITEM = 1,
            MP_INVENTORY = 2
        }

        public enum WALMARTFEEDTYPEPOST
        {
            RETIRE = 1,
            INVENTORYFEED = 2,
            OUTOFSTOCK = 3
        }
    }
}
