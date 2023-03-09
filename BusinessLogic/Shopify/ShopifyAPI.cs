using Newtonsoft.Json;
using RestSharp;
using ShopifyInventorySync.Models;
using System.Net;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Shopify
{
    internal class ShopifyAPI
    {
        ApplicationState applicationState;

        public ShopifyAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        public bool DeleteShopifyProduct(ShopifyInventoryDatum product)
        {
            bool result = false;
            string url = SHOPIFYBASEURL + "/admin/api/2021-10/products/" + product.ShopifyId + ".json";

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Delete);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                RestResponse response = client.Execute(request);

                if (response != null)
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        public ShopifyProductModel CreateNewShopifyItem(ShopifyProductModel shopifyProductModelData)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2021-10/products.json";
            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                var body = JsonConvert.SerializeObject(shopifyProductModelData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                shopifyProductResponseData = JsonConvert.DeserializeObject<ShopifyProductModel>(response.Content!)!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        public Image1 UpdateProductVariantImage(ProductImageAttachVarient shopifyProductImageData, long productId)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2021-10/products/" + productId.ToString() + "/images/" + shopifyProductImageData.image.id + ".json";
            Image1 shopifyProductResponseData = new Image1();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Put);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                var body = JsonConvert.SerializeObject(shopifyProductImageData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                shopifyProductResponseData = JsonConvert.DeserializeObject<Image1>(response.Content!)!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        public NewVariantImageResponseModel CreateProductVariantImage(OverrideVariantImageUpdateModel overrideVariantImageUpdateModel, long productId)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2021-10/products/" + productId.ToString() + "/images.json";
            NewVariantImageResponseModel image = new NewVariantImageResponseModel();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(overrideVariantImageUpdateModel), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    image = JsonConvert.DeserializeObject<NewVariantImageResponseModel>(response.Content!)!;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return image;
        }

        public NewVariantRootModel CreateNewVariant(NewVariantMerge newVariantMerge, string shopifyID)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2022-10/products/" + shopifyID.ToString() + "/variants.json";
            NewVariantRootModel newVariantRootModel = new();

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Post);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(newVariantMerge), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if (response != null)
                {
                    newVariantRootModel = JsonConvert.DeserializeObject<NewVariantRootModel>(response.Content!)!;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return newVariantRootModel;
        }

        public ShopifyProductInventoryResponse SetProductInventoryLevel(ShopifyProductInventoryModel shopifyProductModelData)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2022-01/inventory_levels/set.json";
            ShopifyProductInventoryResponse shopifyProductResponseData = new();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                var body = JsonConvert.SerializeObject(shopifyProductModelData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                shopifyProductResponseData = JsonConvert.DeserializeObject<ShopifyProductInventoryResponse>(response.Content!)!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        public bool UpdateVariantPrice(SingleVariantPriceUpdate singleVariantPriceUpdate)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2022-10/variants/" + singleVariantPriceUpdate.variant.id.ToString() + ".json";
            bool result = false;

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Put);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(singleVariantPriceUpdate), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        public bool AddMetaField(string sku, string shopifyID)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2022-10/products/" + shopifyID + "/metafields.json";
            bool result = false;
            MetafieldModel metafieldModel = new();

            try
            {
                metafieldModel.metafield.@namespace = "mm-google-shopping";
                metafieldModel.metafield.key = "custom_label_0";
                metafieldModel.metafield.value = sku;
                metafieldModel.metafield.type = "single_line_text_field";

                RestClient client = new();
                RestRequest request = new(url, Method.Post);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(metafieldModel), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        public bool OverrideShopifyVariant(OverrideVariantUpdateModel overrideVarianteUpdateModel)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2022-10/variants/" + overrideVarianteUpdateModel.variant.id.ToString() + ".json";
            bool result = false;

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Put);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(overrideVarianteUpdateModel), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        public bool DeleteProductVariantImage(long productId, long imageID)
        {
            string url = SHOPIFYBASEURL + "/admin/api/2021-10/products/" + productId.ToString() + "/images/" + imageID.ToString() + ".json";
            bool result = false;

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url, Method.Delete);
                request.AddHeader("X-Shopify-Access-Token", SHOPIFYACCESSKEY);
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }
    }
}
