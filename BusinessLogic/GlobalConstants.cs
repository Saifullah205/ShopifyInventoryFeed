namespace ShopifyInventorySync.BusinessLogic
{
    public static class GlobalConstants
    {
        public static string TPSSKUPREFIX { get; set; }
        public static string MINIMUMQUANTITY { get; set; }
        public static string MARKOUTOFSTOCK { get; set; }
        public static string LOCATIONID { get; set; }
        public static string SHOPIFYACCESSKEY { get; set; }
        public static string SHOPIFYBASEURL { get; set; }
        public static string FRAGRANCEXSKUPREFIX { get; set; }
        public static string FRAGRANCENETSKUPREFIX { get; set; }
        public static string APIACCESSID { get; set; }
        public static string APIACCESSKEY { get; set; }
        public static string GRANT_TYPE { get; set; }
        public static string FRAGRANCEXURL { get; set; }
        public static string FRAGRANCENETURL { get; set; }
        public static string FRAGRANCENETUSERNAME { get; set; }
        public static string FRAGRANCENETPASSWORD { get; set; }
        public static bool REQUIRESSHIPPING { get; set; }
        public static string WMCONSUMERCHANNELTYPE { get; set; }
        public static string WMQOSCORRELATIONID { get; set; }
        public static string WMSVCNAME { get; set; }
        public static string WALMARTAUTHORIZATION { get; set; }
        public static string WALMARTURL { get; set; }
        public static string SHIPPINGTEMPLATEID { get; set; }
        public static string FULFILLMENTCENTERID { get; set; }
        public static string WALMARTCHUNKSIZE { get; set; }
        public static string WALMRTTOKEN { get; set; }
        public static string WALMARTMINPRICELEVEL { get; set; }
        public static string WALMARTMINORDERQTY { get; set; }

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
            MP_INVENTORY = 2,
            MP_SHIPPINGMAP = 3
        }

        public enum WALMARTFEEDTYPEPOST
        {
            RETIRE = 1,
            SETUPITEM = 2,
            MAPSHIPPINGTEMPLATE = 3,
            INVENTORYFEED = 4
        }
    }
}
