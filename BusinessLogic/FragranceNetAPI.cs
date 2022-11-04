﻿using CsvHelper;
using RestSharp;
using RestSharp.Authenticators;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{
    internal class FragranceNetAPI
    {

        ApplicationState applicationState;

        private readonly IProductsRepository productsRepository;

        public FragranceNetAPI(IProductsRepository productsRepository)
        {
            applicationState = ApplicationState.GetState;

            this.productsRepository = productsRepository;
        }

        private string fetchDataFromAPI()
        {
            string responseData = String.Empty;
            FileInfo fileInfo;

            try
            {
                fileInfo = applicationState.ShowBrowseFileDialog();

                if (fileInfo != null)
                {
                    responseData = File.ReadAllText(fileInfo.FullName);
                }

                if (string.IsNullOrEmpty(responseData))
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return responseData;
        }

        public FragranceNetProductsList GetDataFromSource()
        {
            string fileTextData;
            string csvTextData;
            FragranceNetProductsList fragranceNetProducts = new();
            byte[] csvBytes;

            try
            {
                fileTextData = fetchDataFromAPI();

                if (!string.IsNullOrEmpty(fileTextData))
                {
                    fileTextData = fileTextData.Replace("\"", "");
                    fileTextData = "\"" + fileTextData;
                    fileTextData = fileTextData.Replace("\t", "\",\"");
                    csvTextData = fileTextData.Replace("\n", "\"\n\"");

                    csvTextData = csvTextData.Substring(0, csvTextData.Length - 1);

                    csvBytes = Encoding.UTF8.GetBytes(csvTextData);

                    using (var reader = new StreamReader(new MemoryStream(csvBytes)))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        fragranceNetProducts.products = csv.GetRecords<FragranceNetProduct>().ToList<FragranceNetProduct>();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return fragranceNetProducts;
        }

        public List<ShopifyInventoryDatum> FilterRemovedProducts(FragranceNetProductsList fragranceNetProducts)
        {
            List<ShopifyInventoryDatum> shopifyProductsToRemove = new List<ShopifyInventoryDatum>();
            List<FragranceNetProduct> products = new List<FragranceNetProduct>();

            try
            {
                products = fragranceNetProducts.products;

                shopifyProductsToRemove = (from s in this.productsRepository.GetBySkuPrefix(GlobalConstants.fragranceNetSKUPrefix)
                                           where !products.Any(x => x.sku == s.Sku && s.SkuPrefix == GlobalConstants.fragranceNetSKUPrefix)
                                           select s).ToList<ShopifyInventoryDatum>();
            }
            catch (Exception)
            {
                throw;
            }

            return shopifyProductsToRemove;
        }

        public List<FragranceNetProductsList> FormatSourceProductsData(FragranceNetProductsList fragranceNetProducts)
        {
            List<FragranceNetProduct> productsListPrePrepare = new List<FragranceNetProduct>();
            List<FragranceNetProduct> productsListPostPrepare = new List<FragranceNetProduct>();
            List<FragranceNetProductsList> productsDataListPrepared = new List<FragranceNetProductsList>();
            FragranceNetProduct selectedProduct = new();
            string productName = string.Empty;
            string gender = string.Empty;

            try
            {
                productsDataListPrepared.Clear();

                productsListPrePrepare = fragranceNetProducts.products;

                while (productsListPrePrepare.Count > 0)
                {
                    FragranceNetProductsList csvProductsToProcessModel = new();

                    selectedProduct = productsListPrePrepare.First();

                    productName = selectedProduct.name;
                    gender = selectedProduct.gender;

                    productsListPostPrepare = productsListPrePrepare.Where(m => m.name == productName && m.gender == gender).ToList<FragranceNetProduct>();

                    csvProductsToProcessModel.products = productsListPostPrepare;

                    productsDataListPrepared.Add(csvProductsToProcessModel);

                    productsListPrePrepare.RemoveAll(m => m.name == productName && m.gender == gender);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsDataListPrepared;
        }

        public void ProcessProductToShopify(FragranceNetProductsList productsToProcessData)
        {
            string shopifyID = string.Empty;
            string sku = string.Empty;
            string mainTitle = string.Empty;
            string variantTitle = string.Empty;
            string weight = string.Empty;
            string weightDescription = string.Empty;
            string imageURL = string.Empty;
            string vendor = string.Empty;
            string productDescription = string.Empty;
            string skuPrefix = string.Empty;
            string fullSku = string.Empty;
            string minimumQty = string.Empty;
            string gender = string.Empty;
            string genderF = string.Empty;
            string genderDescription = string.Empty;
            string giftSet = string.Empty;
            bool isVariantAdded = false;
            Option WeightOption = new();
            Option GenderOption = new();
            List<string> restrictedSKus = new();
            FragranceNetProduct headerProduct = new();

            ShopifyDbContext shopifyDBContextUpdateProduct = new ShopifyDbContext();

            ShopifyInventoryDatum currentProduct = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new List<ShopifyInventoryDatum>();
            ShopifyProductModel shopifyProductModelData = new ShopifyProductModel();
            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                headerProduct = productsToProcessData.products[0];

                skuPrefix = GlobalConstants.fragranceNetSKUPrefix;

                mainTitle = headerProduct.name;
                vendor = headerProduct.brand;
                productDescription = headerProduct.productDescription + " " + headerProduct.fragranceNotes + " " + headerProduct.recommendedUse;

                mainTitle = mainTitle + " by " + vendor;

                //if (restrictedBrandsList.Where(m => m.BrandName == vendor).ToList<RestrictedBrand>().Count > 0)
                //{
                //    AddMessageToLogs(Convert.ToString(vendor + " : Restricted Brand Found"));

                //    restrictedSKus = (productsToProcessData.products.Where(m => m.BrandName == vendor)).Select(m => m.Upc).ToList<string>();

                //    UpdateSKUAsRestricted(restrictedSKus);

                //    return;
                //}

                shopifyProductModelData.product.title = mainTitle;
                shopifyProductModelData.product.body_html = string.IsNullOrEmpty(productDescription) ? mainTitle : productDescription;
                shopifyProductModelData.product.vendor = vendor;
                shopifyProductModelData.product.status = "active";
                shopifyProductModelData.product.published_at = DateTime.Now;

                GenderOption.name = "Gender";
                GenderOption.position = 2;

                WeightOption.name = "Product";
                WeightOption.position = 1;

                //foreach (FragranceNetProduct productData in productsToProcessData.products)
                //{
                //    Image1 image = new();
                //    ShopifyFixedPrice shopifyFixedPrice = new();
                //    bool isFixedPrice = false;
                //    string cost = string.Empty;
                //    string updatedCost = string.Empty;

                //    sku = productData.Upc.ToString()!;
                //    weight = "0";
                //    cost = productData.WholesalePriceUSD.ToString()!;
                //    imageURL = productData.LargeImageUrl.ToString()!;
                //    gender = productData.Gender.ToString()!;
                //    minimumQty = productData.QuantityAvailable.ToString();

                //    shopifyFixedPrice = shopifyFixedPricesList.Where(m => m.Sku == sku).ToList<ShopifyFixedPrice>().FirstOrDefault();

                //    if (shopifyFixedPrice != null)
                //    {
                //        try
                //        {
                //            if (Convert.ToDecimal(shopifyFixedPrice!.FixedPrice) > 0)
                //            {
                //                cost = shopifyFixedPrice.FixedPrice!;

                //                isFixedPrice = true;
                //            }
                //        }
                //        catch (Exception)
                //        {
                //            isFixedPrice = false;
                //        }
                //    }

                //    if (isFixedPrice)
                //    {
                //        updatedCost = cost;
                //    }
                //    else
                //    {
                //        updatedCost = Convert.ToString(CalculateMarkupPrice(Convert.ToDecimal(cost)));
                //    }

                //    if (productData.Type.ToUpper().Contains("UNISEX") || productData.Description.ToUpper().Contains("UNISEX"))
                //    {
                //        gender = "UNISEX";
                //    }

                //    fullSku = skuPrefix + sku.Trim();

                //    currentProduct = shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().FirstOrDefault();

                //    if (restrictedSkusList.Where(m => m.Sku == sku).ToList<RestrictedSku>().Count > 0)
                //    {
                //        AddMessageToLogs(Convert.ToString(fullSku + " : Restricted SKU Found"));

                //        restrictedSKus.Clear();

                //        restrictedSKus.Add(sku);

                //        UpdateSKUAsRestricted(restrictedSKus);

                //        continue;
                //    }

                //    if (gender.ToUpper() == "MEN")
                //    {
                //        genderDescription = "Men";
                //    }
                //    else if (gender.ToUpper() == "WOMEN")
                //    {
                //        genderDescription = "Women";
                //    }
                //    else if (gender.ToUpper() == "UNISEX")
                //    {
                //        genderDescription = "Unisex";
                //    }

                //    if (shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().Count <= 0)
                //    {
                //        Variant variant = new();
                //        ShopifyInventoryDatum sameNameProduct = new();

                //        variantTitle = productData.ProductName + " by " + productData.BrandName;
                //        weightDescription = productData.Size + " " + productData.Type;

                //        if (giftSet == "Y")
                //        {
                //            variantTitle = "Gift Set - " + variantTitle;
                //        }

                //        variant.title = variantTitle;
                //        variant.sku = fullSku;
                //        variant.barcode = sku.Trim();
                //        variant.price = updatedCost;
                //        variant.weight = Convert.ToDouble(weight);
                //        variant.inventory_management = "shopify";
                //        variant.inventory_policy = "deny";
                //        variant.fulfillment_service = "manual";
                //        variant.inventory_quantity = Convert.ToInt32(minimumQty);
                //        variant.option1 = weightDescription;
                //        variant.option2 = genderDescription;

                //        image.src = imageURL;

                //        if (!WeightOption.values.Contains("weightDescription"))
                //        {
                //            WeightOption.values.Add(weightDescription);
                //        }

                //        if (!WeightOption.values.Contains("genderDescription"))
                //        {
                //            GenderOption.values.Add(genderDescription);
                //        }

                //        sameNameProduct = shopifyProductsData.Where(m => m.ProductName == variantTitle && m.ProductGender == genderDescription).FirstOrDefault()!;

                //        if (sameNameProduct != null)
                //        {
                //            NewVariantRootModel newVariantRootModel = new();
                //            NewVariantMerge newVariantMerge = new();
                //            OverrideVariantImageUpdateModel overrideVariantImageUpdateModel = new();
                //            NewVariantImageResponseModel newImage = new();
                //            ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();
                //            long[] variantIds;
                //            NewVariantRequest newVariantRequest = new();

                //            try
                //            {
                //                newVariantRequest.title = variant.title;
                //                newVariantRequest.sku = variant.sku;
                //                newVariantRequest.barcode = variant.sku;
                //                newVariantRequest.price = variant.price;
                //                newVariantRequest.weight = variant.weight;
                //                newVariantRequest.inventory_management = variant.inventory_management;
                //                newVariantRequest.inventory_policy = variant.inventory_policy;
                //                newVariantRequest.fulfillment_service = variant.fulfillment_service;
                //                newVariantRequest.option1 = variant.option1;
                //                newVariantRequest.option2 = variant.option2;

                //                newVariantMerge.variant = newVariantRequest;

                //                newVariantRootModel = CreateNewVariant(newVariantMerge, sameNameProduct.ShopifyId!.ToString());

                //                variantIds = new long[] { Convert.ToInt64(newVariantRootModel.variant.id) };

                //                overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(sameNameProduct.ShopifyId);
                //                overrideVariantImageUpdateModel.image.src = imageURL;
                //                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                //                newImage = CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                //                shopifyInventoryDatum.ProductName = mainTitle.Split(",")[0];
                //                shopifyInventoryDatum.ProductGender = genderDescription;
                //                shopifyInventoryDatum.ShopifyId = sameNameProduct.ShopifyId.ToString();
                //                shopifyInventoryDatum.BrandName = sameNameProduct.BrandName;
                //                shopifyInventoryDatum.Sku = sku.Trim();
                //                shopifyInventoryDatum.SkuPrefix = skuPrefix;
                //                shopifyInventoryDatum.VariantId = newVariantRootModel.variant.id.ToString();
                //                shopifyInventoryDatum.InventoryItemId = newVariantRootModel.variant.inventory_item_id.ToString();
                //                shopifyInventoryDatum.ImageId = newImage.image.id.ToString();

                //                shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDatum);

                //                shopifyDBContextUpdateProduct.SaveChanges();

                //                RefreshShopifySkusList();

                //                UpdateProductStockQuantity(sku, 50);
                //            }
                //            catch (Exception ex)
                //            {
                //                applicationState.LogErrorToFile(ex);
                //            }
                //        }
                //        else
                //        {
                //            shopifyProductModelData.product.variants.Add(variant);
                //            shopifyProductModelData.product.images.Add(image);

                //            isVariantAdded = true;
                //        }

                //        AddMessageToLogs(fullSku + " : SKU merged");
                //    }
                //    else if (currentProduct!.SkuPrefix == fraganceXSkuPrefix)
                //    {
                //        UpdateProductStockQuantity(sku, productData.QuantityAvailable);
                //        UpdateProductNewPrice(sku, currentProduct.VariantId!.ToString(), Convert.ToDecimal(updatedCost));
                //    }
                //}

                //if (isVariantAdded)
                //{
                //    shopifyProductModelData.product.options.Add(WeightOption);
                //    shopifyProductModelData.product.options.Add(GenderOption);

                //    shopifyProductResponseData = CreateNewShopifyItem(shopifyProductModelData);

                //    if (shopifyProductResponseData.product.variants.Count > 0)
                //    {
                //        List<ProductImageAttachVarient> productImageAttachVarientsList = new();

                //        productImageAttachVarientsList = UpdateFragranceXProductVarientImages(shopifyProductResponseData, productsToProcessData);

                //        AddMetaField(sku, shopifyProductResponseData.product.id.ToString());

                //        foreach (Variant productVarient in shopifyProductResponseData.product.variants)
                //        {
                //            ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();

                //            shopifyInventoryDatum.ProductName = mainTitle;
                //            shopifyInventoryDatum.ProductGender = genderDescription;
                //            shopifyInventoryDatum.ShopifyId = shopifyProductResponseData.product.id.ToString();
                //            shopifyInventoryDatum.BrandName = shopifyProductResponseData.product.vendor;
                //            shopifyInventoryDatum.Sku = productVarient.sku.Substring(fraganceXSkuPrefix.Length, productVarient.sku.Length - fraganceXSkuPrefix.Length);
                //            shopifyInventoryDatum.SkuPrefix = skuPrefix;
                //            shopifyInventoryDatum.VariantId = productVarient.id.ToString();
                //            shopifyInventoryDatum.InventoryItemId = productVarient.inventory_item_id.ToString();

                //            try
                //            {
                //                shopifyInventoryDatum.ImageId = productImageAttachVarientsList.Where(m => m.image.variant_ids.Contains(productVarient.id)).FirstOrDefault()!.image.id.ToString();
                //            }
                //            catch (Exception ex)
                //            {
                //                applicationState.LogErrorToFile(ex);
                //            }


                //            shopifyInventoryDataList.Add(shopifyInventoryDatum);
                //        }

                //        if (shopifyInventoryDataList.Count > 0)
                //        {
                //            shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDataList);

                //            shopifyDBContextUpdateProduct.SaveChanges();

                //            AddMessageToLogs(Convert.ToString("--------New product created successfully--------"));
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }
    }
}
