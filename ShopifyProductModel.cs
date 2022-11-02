using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync
{
    public class ShopifyProductModel
    {
        public ShopifyProductModel()
        {
            product = new();
        }

        public Product product { get; set; }
    }

    public class Product
    {
        public Product()
        {
            variants = new List<Variant>();
            options = new List<Option>();
            images = new List<Image1>();
        }

        public long id { get; set; }
        public string title { get; set; }
        public string body_html { get; set; }
        public string vendor { get; set; }
        public string product_type { get; set; }
        public DateTime created_at { get; set; }
        public string handle { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime published_at { get; set; }
        public object template_suffix { get; set; }
        public string status { get; set; }
        public string published_scope { get; set; }
        public string tags { get; set; }
        public string admin_graphql_api_id { get; set; }
        public List<Variant> variants { get; set; }
        public List<Option> options { get; set; }
        public List<Image1> images { get; set; }
        public Image image { get; set; }
    }

    public class Image
    {
        public Image()
        {
            variant_ids = new List<long>();
        }

        public long product_id { get; set; }
        public long id { get; set; }
        public int position { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public List<long> variant_ids { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class Variant
    {
        public long product_id { get; set; }
        public long id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
        public int position { get; set; }
        public string inventory_policy { get; set; }
        public object compare_at_price { get; set; }
        public string fulfillment_service { get; set; }
        public string inventory_management { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public object option3 { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool taxable { get; set; }
        public string barcode { get; set; }
        public int grams { get; set; }
        public long? image_id { get; set; }
        public double weight { get; set; }
        public string weight_unit { get; set; }
        public long inventory_item_id { get; set; }
        public int inventory_quantity { get; set; }
        public int old_inventory_quantity { get; set; }
        public bool requires_shipping { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class Option
    {
        public Option()
        {
            values = new List<string>();
        }

        public long product_id { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public int position { get; set; }
        public List<string> values { get; set; }
    }

    public class Image1
    {
        public Image1()
        {
            variant_ids = new List<long>();
        }

        public long product_id { get; set; }
        public long id { get; set; }
        public int position { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object alt { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public List<long> variant_ids { get; set; }
        public string admin_graphql_api_id { get; set; }
    }


    public class ShopifyProductInventoryModel
    {
        public long location_id { get; set; }
        public long inventory_item_id { get; set; }
        public int available { get; set; }
    }


    public class ShopifyProductInventoryResponse
    {
        public ShopifyProductInventoryResponse()
        {
            inventory_level = new Inventory_Level();
        }
        public Inventory_Level inventory_level { get; set; }
    }

    public class Inventory_Level
    {
        public long inventory_item_id { get; set; }
        public long location_id { get; set; }
        public int available { get; set; }
        public DateTime updated_at { get; set; }
        public string? admin_graphql_api_id { get; set; }
    }


    public class ProductImageAttachVarient
    {
        public ProductImageAttachVarient()
        {
            image = new();
        }
        public VarientImage image { get; set; }
    }

    public class VarientImage
    {
        public long id { get; set; }
        public long[] variant_ids { get; set; }
    }


    public class SingleVariantPriceUpdate
    {
        public SingleVariantPriceUpdate()
        {
            variant = new();
        }

        public SingleVariant variant { get; set; }
    }

    public class SingleVariant
    {
        public long id { get; set; }
        public string price { get; set; }
    }

    public class OverrideVariantUpdateModel
    {
        public OverrideVariantUpdateModel()
        {
            variant = new();
        }

        public OverrideVariant variant { get; set; }
    }

    public class OverrideVariant
    {
        public long id { get; set; }
        public string sku { get; set; }
        public string price { get; set; }
        public string title { get; set; }
        public string option1 { get; set; }
    }


    public class OverrideVariantImageUpdateModel
    {
        public OverrideVariantImageUpdateModel()
        {
            image = new();
        }

        public OverrideVariantImage image { get; set; }
    }

    public class OverrideVariantImage
    {
        public long product_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string src { get; set; }
        public long[] variant_ids { get; set; }
    }

    public class NewVariantImageResponseModel
    {
        public NewVariantImageResponseModel()
        {
            image = new();
        }

        public Image1 image { get; set; }
    }

    public class NewVariantMerge
    {
        public NewVariantMerge()
        {
            variant = new();
        }

        public NewVariantRequest variant { get; set; }
    }


    public class NewVariantRequest
    {
        public long product_id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
        public string barcode { get; set; }
        public string inventory_policy { get; set; }
        public object compare_at_price { get; set; }
        public string fulfillment_service { get; set; }
        public string inventory_management { get; set; }
        public string option1 { get; set; }
        public string option2 { get; set; }
        public double weight { get; set; }
    }

    public class NewVariantRootModel
    {
        public NewVariantRootModel()
        {
            variant = new();
        }
        public NewVariantResponse variant { get; set; }
    }

    public class NewVariantResponse
    {
        public long id { get; set; }
        public long product_id { get; set; }
        public string title { get; set; }
        public string price { get; set; }
        public string sku { get; set; }
        public long position { get; set; }
        public string inventory_policy { get; set; }
        public object compare_at_price { get; set; }
        public string fulfillment_service { get; set; }
        public string inventory_management { get; set; }
        public string option1 { get; set; }
        public object option2 { get; set; }
        public object option3 { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool taxable { get; set; }
        public object barcode { get; set; }
        public long grams { get; set; }
        public long? image_id { get; set; }
        public long weight { get; set; }
        public string weight_unit { get; set; }
        public long inventory_item_id { get; set; }
        public long inventory_quantity { get; set; }
        public long old_inventory_quantity { get; set; }
        public Presentment_Prices[] presentment_prices { get; set; }
        public bool requires_shipping { get; set; }
        public string admin_graphql_api_id { get; set; }
    }

    public class Presentment_Prices
    {
        public Presentment_Prices()
        {
            price = new();
        }
        public Price price { get; set; }
        public object compare_at_price { get; set; }
    }

    public class Price
    {
        public string amount { get; set; }
        public string currency_code { get; set; }
    }


    public class MetafieldModel
    {
        public MetafieldModel()
        {
            metafield = new();
        }

        public Metafield metafield { get; set; }
    }

    public class Metafield
    {
        public string @namespace { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

}
