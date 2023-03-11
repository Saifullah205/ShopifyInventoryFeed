using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{

    public class WalmartTokenModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    #region Walmart_Product_Model
    public class WalmartProductModel
    {
        public WalmartProductModel()
        {
            MPItemFeedHeader = new();
            MPItem = new();
        }

        public Mpitemfeedheader MPItemFeedHeader { get; set; }
        public List<Mpitem> MPItem { get; set; }
    }

    public class Mpitemfeedheader
    {
        public string sellingChannel { get; set; }
        public string processMode { get; set; }
        public string subset { get; set; }
        public string locale { get; set; }
        public string version { get; set; }
        public string subCategory { get; set; }
    }

    public class Mpitem
    {
        public Mpitem()
        {
            Orderable = new();
            Visible = new();
        }

        public Orderable Orderable { get; set; }
        public Visible Visible { get; set; }
    }

    public class Orderable
    {
        public Orderable()
        {
            productIdentifiers = new();
        }

        public string sku { get; set; }
        public Productidentifiers productIdentifiers { get; set; }
        public string productName { get; set; }
        public string brand { get; set; }
        public decimal price { get; set; }
        public int ShippingWeight { get; set; }
        public string electronicsIndicator { get; set; }
        public string batteryTechnologyType { get; set; }
        public string chemicalAerosolPesticide { get; set; }
        public string shipsInOriginalPackaging { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string MustShipAlone { get; set; }
    }

    public class Productidentifiers
    {
        public string productIdType { get; set; }
        public string productId { get; set; }
    }

    public class Visible
    {
        public Visible()
        {
            Office = new();
        }

        public Office Office { get; set; }
    }

    public class Office
    {
        public Office()
        {
            productSecondaryImageURL = new();
            smallPartsWarnings = new();
            keyFeatures = new();
        }

        public string shortDescription { get; set; }
        public string mainImageUrl { get; set; }
        public List<string> productSecondaryImageURL { get; set; }
        public string prop65WarningText { get; set; }
        public List<string> smallPartsWarnings { get; set; }
        public string compositeWoodCertificationCode { get; set; }
        public List<string> keyFeatures { get; set; }
        public string manufacturer { get; set; }
        public string manufacturerPartNumber { get; set; }
    }

    public class WalmartRetireItemResponseModel
    {
        public string sku { get; set; }
        public string message { get; set; }
        public object additionalAttributes { get; set; }
        public object errors { get; set; }
    }
    #endregion

    #region WalmartInventoryRequestModel


    public class WalmartInventoryRequestModel
    {
        public WalmartInventoryRequestModel()
        {
            inventoryHeader = new();
            inventory = new();
        }

        public Inventoryheader inventoryHeader { get; set; }
        public List<Inventory> inventory { get; set; }
    }

    public class Inventoryheader
    {
        public string version { get; set; }
    }

    public class Inventory
    {
        public Inventory()
        {
            shipNodes = new();
        }

        public string sku { get; set; }
        public List<Shipnode> shipNodes { get; set; }
    }

    public class Shipnode
    {
        public Shipnode()
        {
            quantity = new();
        }

        public string shipNode { get; set; }
        public Quantity quantity { get; set; }
    }

    public class Quantity
    {
        public string unit { get; set; }
        public int amount { get; set; }
    }


    #endregion

    public class WalmartFeedObject
    {
        public string feedId { get; set; }
    }

    #region Shipping_Template_Mapping

    public class WalmartShippingTemplateMapping
    {
        public WalmartShippingTemplateMapping()
        {
            ItemFeedHeader = new();
            Item = new();
        }

        public Itemfeedheader ItemFeedHeader { get; set; }
        public List<ShippingTemplateItem> Item { get; set; }
    }

    public class Itemfeedheader
    {
        public string sellingChannel { get; set; }
        public string locale { get; set; }
        public string version { get; set; }
    }

    public class ShippingTemplateItem
    {
        public ShippingTemplateItem()
        {
            PreciseDelivery = new();
        }

        public Precisedelivery PreciseDelivery { get; set; }
    }

    public class Precisedelivery
    {
        public string sku { get; set; }
        public string actionType { get; set; }
        public string shippingTemplateId { get; set; }
        public string fulfillmentCenterId { get; set; }
    }

    #endregion

    #region Walmart_Feed_Response

    public class WalmartFeedResponseModel
    {
        public WalmartFeedResponseModel()
        {
            itemDetails = new();
        }

        public string feedId { get; set; }
        public string feedStatus { get; set; }
        public object shipNode { get; set; }
        public object submittedBy { get; set; }
        public long feedSubmissionDate { get; set; }
        public object ingestionErrors { get; set; }
        public int itemsReceived { get; set; }
        public int itemsSucceeded { get; set; }
        public int itemsFailed { get; set; }
        public int itemsProcessing { get; set; }
        public int offset { get; set; }
        public int limit { get; set; }
        public Itemdetails itemDetails { get; set; }
        public object additionalAttributes { get; set; }
    }

    public class Itemdetails
    {
        public Itemdetails()
        {
            itemIngestionStatus = new();
        }

        public List<Itemingestionstats> itemIngestionStatus { get; set; }
    }

    public class Itemingestionstats
    {
        public Itemingestionstats()
        {
            productIdentifiers = new();
        }

        public int martId { get; set; }
        public string sku { get; set; }
        public object wpid { get; set; }
        public int index { get; set; }
        public string itemid { get; set; }
        public object shipNode { get; set; }
        public Productidentifiers_WMTRes productIdentifiers { get; set; }
        public string ingestionStatus { get; set; }
        public object ingestionErrors { get; set; }
        public object additionalAttributes { get; set; }
    }

    public class Productidentifiers_WMTRes
    {
        public object productIdentifier { get; set; }
    }



    #endregion
}
