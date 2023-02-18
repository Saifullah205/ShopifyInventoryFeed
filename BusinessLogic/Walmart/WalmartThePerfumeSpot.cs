using CsvHelper;
using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic.Shopify;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    internal class WalmartThePerfumeSpot
    {
        ApplicationState applicationState;

        private readonly IWalmartInventoryDataRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly ShopifyAPI shopifyAPI;
        private readonly WalmartAPI walmartAPI;

        public WalmartThePerfumeSpot()
        {
            applicationState = ApplicationState.GetState;
            shopifyAPI = new();
            walmartAPI = new();
            productsRepository = new WalmartInventoryDataRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();
        }

        private string FetchDataFromAPI()
        {
            string responseData = string.Empty;
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

        public ThePerfumeSpotProductsList GetDataFromSource()
        {
            string fileTextData;
            ThePerfumeSpotProductsList fragranceNetProducts = new();
            byte[] csvBytes;

            try
            {
                fileTextData = FetchDataFromAPI();

                if (!string.IsNullOrEmpty(fileTextData))
                {
                    csvBytes = Encoding.UTF8.GetBytes(fileTextData);

                    using (var reader = new StreamReader(new MemoryStream(csvBytes)))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        fragranceNetProducts.products = csv.GetRecords<ThePerfumeSpotProduct>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceNetProducts;
        }

        public List<WalmartInventoryDatum> FilterOutOfStockProducts(ThePerfumeSpotProductsList productsList)
        {
            List<WalmartInventoryDatum> productsToRemove = new();
            List<ThePerfumeSpotProduct> products = new();

            try
            {
                products = productsList.products;

                productsToRemove = (from s in productsRepository.GetBySkuPrefix(GlobalConstants.tpsSKUPrefix)
                                    where !products.Any(x => x.UPC == s.Sku)
                                    select s).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<ThePerfumeSpotProduct> FilterProductsToRemove(ThePerfumeSpotProductsList productsList)
        {
            List<ThePerfumeSpotProduct> productsToRemove = new();
            List<RestrictedBrand> restrictedBrands = new List<RestrictedBrand>();
            List<RestrictedSku> restrictedSku = new List<RestrictedSku>();

            try
            {
                restrictedBrands = (from s in restrictedBrandsRepository.GetAll()
                                    where (s.EcomStoreId == (int)GlobalConstants.STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == "SBB"))
                                    select s).ToList<RestrictedBrand>();

                restrictedSku = (from s in restrictedSkusRepository.GetAll()
                                 where (s.EcomStoreId == (int)GlobalConstants.STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == "SBB"))
                                 select s).ToList<RestrictedSku>();

                productsToRemove = (from s in productsList.products
                                    where restrictedSku.Any(x => x.Sku == s.UPC) || restrictedBrands.Any(x => x.BrandName == s.Brand)
                                    select s).ToList<ThePerfumeSpotProduct>();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<ThePerfumeSpotProduct> FilterProductsToProcess(ThePerfumeSpotProductsList productsList, List<ThePerfumeSpotProduct> removedProducts)
        {
            List<ThePerfumeSpotProduct> productsToProcess = new();

            try
            {
                productsToProcess = (from s in productsList.products
                                    where !removedProducts.Any(x => x.UPC == s.UPC)
                                    select s).ToList<ThePerfumeSpotProduct>();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToProcess;
        }

        public List<WalmartInventoryDatum> PrepareInStockProductsQtyToProcess(List<ThePerfumeSpotProduct> productsList)
        {
            List<WalmartInventoryDatum> productsToProcess = new();

            try
            {
                foreach (ThePerfumeSpotProduct item in productsList)
                {
                    WalmartInventoryDatum thePerfumeSpotProduct = new();

                    thePerfumeSpotProduct.Sku = item.UPC;

                    productsToProcess.Add(thePerfumeSpotProduct);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsToProcess;
        }

        public List<string> FormatSourceProductsData(List<ThePerfumeSpotProduct> productsList)
        {
            List<ThePerfumeSpotProduct> productsListPrePrepare = new();
            List<ThePerfumeSpotProduct> productsListPostPrepare = new();
            List<ThePerfumeSpotProductsList> productsDataListPrepared = new();
            ThePerfumeSpotProduct selectedProduct = new();
            string productName = string.Empty;
            string genderMale = string.Empty;
            string genderFemale = string.Empty;
            WalmartProductModel walmartProductModel = new();
            List<string> productsData = new List<string>();
            IEnumerable<ThePerfumeSpotProduct[]> thePerfumeSpotProductsMultiLists;

            try
            {
                thePerfumeSpotProductsMultiLists = productsList.Chunk(8000);

                walmartProductModel.MPItemFeedHeader.sellingChannel = "marketplace";
                walmartProductModel.MPItemFeedHeader.processMode = "REPLACE";
                walmartProductModel.MPItemFeedHeader.subset = "EXTERNAL";
                walmartProductModel.MPItemFeedHeader.locale = "en";
                walmartProductModel.MPItemFeedHeader.version = "1.5";
                walmartProductModel.MPItemFeedHeader.subCategory = "office_other";

                foreach (ThePerfumeSpotProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartProductModel.MPItem.Clear();

                    foreach (ThePerfumeSpotProduct productData in productsSingleList)
                    {
                        Mpitem mpitem = new();
                        string sku = string.Empty;
                        string fullSku = string.Empty;
                        List<string> restrictedSKus = new();
                        string vendor = string.Empty;
                        string mainTitle = string.Empty;

                        sku = productData.UPC;
                        fullSku = GlobalConstants.tpsSKUPrefix + productData.UPC;
                        vendor = productData.Brand;
                        mainTitle = productData.Name.Split(',')[0].ToString();

                        restrictedSKus = new List<string> { productData.UPC };

                        if (!ValidateRestrictedBrand(vendor, restrictedSKus))
                        {
                            continue;
                        }

                        mpitem.Orderable.sku = fullSku;
                        mpitem.Orderable.productIdentifiers.productIdType = "GTIN";
                        mpitem.Orderable.productIdentifiers.productId = sku.PadLeft(14, '0');

                        mpitem.Orderable.productName = mainTitle;
                        mpitem.Orderable.brand = vendor;
                        mpitem.Orderable.price = CalculateMarkedUpPrice(sku, productData.Retail);
                        mpitem.Orderable.ShippingWeight = (int)Convert.ToDecimal(string.IsNullOrEmpty(productData.Weight) ? "0" : productData.Weight);
                        mpitem.Orderable.electronicsIndicator = "No";
                        mpitem.Orderable.batteryTechnologyType = "Does Not Contain a Battery";
                        mpitem.Orderable.chemicalAerosolPesticide = "No";
                        mpitem.Orderable.shipsInOriginalPackaging = "No";
                        mpitem.Orderable.startDate = "2019-01-01T08:00:00Z";
                        mpitem.Orderable.endDate = "2060-01-01T08:00:00Z";
                        mpitem.Orderable.MustShipAlone = "No";

                        mpitem.Visible.Office.shortDescription = "Storage";
                        mpitem.Visible.Office.mainImageUrl = productData.ImageURL;
                        mpitem.Visible.Office.productSecondaryImageURL = new List<string> { productData.ImageURL };
                        mpitem.Visible.Office.prop65WarningText = "None";
                        mpitem.Visible.Office.smallPartsWarnings = new List<string> { "0 - No warning applicable" };
                        mpitem.Visible.Office.compositeWoodCertificationCode = "1 - Does not contain composite wood";
                        mpitem.Visible.Office.keyFeatures = new List<string> { productData.Name };
                        mpitem.Visible.Office.manufacturer = productData.Brand;
                        mpitem.Visible.Office.manufacturerPartNumber = productData.SKU;

                        walmartProductModel.MPItem.Add(mpitem);                        
                    }

                    productsData.Add(JsonConvert.SerializeObject(walmartProductModel));

                    walmartProductModel.MPItem.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsData;
        }

        public List<string> FormatSourceProductsInventoryData(List<WalmartInventoryDatum> productsList, bool markOutOfStock)
        {
            WalmartInventoryRequestModel walmartInventoryRequestModel = new();
            IEnumerable<WalmartInventoryDatum[]> thePerfumeSpotProductsMultiLists;
            List<string> productsData = new List<string>();

            try
            {
                thePerfumeSpotProductsMultiLists = productsList.Chunk(500);

                walmartInventoryRequestModel.InventoryHeader.version = "1.4";

                foreach (WalmartInventoryDatum[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartInventoryRequestModel.Inventory.Clear();

                    foreach (WalmartInventoryDatum productData in productsSingleList)
                    {
                        Inventory inventory = new();

                        inventory.sku = GlobalConstants.tpsSKUPrefix + productData.Sku;
                        inventory.quantity.unit = "EACH";
                        inventory.quantity.amount = markOutOfStock ? 0 : Convert.ToInt32(GlobalConstants.minimumQuantity);

                        walmartInventoryRequestModel.Inventory.Add(inventory);
                    }

                    productsData.Add(JsonConvert.SerializeObject(walmartInventoryRequestModel));
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsData;
        }

        public void ProcessProductToShopify(ThePerfumeSpotProductsList productsToProcessData)
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
            string genderM = string.Empty;
            string genderF = string.Empty;
            string genderDescription = string.Empty;
            string giftSet = string.Empty;
            bool isVariantAdded = false;
            Option WeightOption = new();
            Option GenderOption = new();
            List<string> restrictedSKus = new();
            ThePerfumeSpotProduct headerProduct = new();
            ProductsRepository productsRepositoryContext = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new();
            ShopifyProductModel shopifyProductModelData = new();
            ShopifyProductModel shopifyProductResponseData = new();

            try
            {
                headerProduct = productsToProcessData.products[0];

                skuPrefix = GlobalConstants.tpsSKUPrefix;

                mainTitle = headerProduct.Name.Split(',')[0].ToString();
                vendor = headerProduct.Brand;
                productDescription = headerProduct.Name;

                restrictedSKus = productsToProcessData.products.Select(m => m.UPC).ToList();

                if (!ValidateRestrictedBrand(vendor, restrictedSKus))
                {
                    return;
                }

                shopifyProductModelData.product.title = mainTitle;
                shopifyProductModelData.product.body_html = string.IsNullOrEmpty(productDescription) ? mainTitle : productDescription;
                shopifyProductModelData.product.vendor = vendor;
                shopifyProductModelData.product.status = "active";
                shopifyProductModelData.product.published_at = DateTime.Now;

                GenderOption.name = "Gender";
                GenderOption.position = 2;

                WeightOption.name = "Product";
                WeightOption.position = 1;

                foreach (ThePerfumeSpotProduct productData in productsToProcessData.products)
                {
                    ShopifyInventoryDatum? currentProduct = new();
                    FixedPrice? shopifyFixedPrice = new();
                    Image1 image = new();
                    bool isFixedPrice = false;
                    bool isSKUReplaced = false;
                    string cost = string.Empty;
                    string updatedCost = string.Empty;

                    variantTitle = productData.Name.Split(',')[0].ToString();
                    sku = productData.UPC.ToString()!;
                    fullSku = skuPrefix + sku.Trim();
                    weight = productData.Weight.ToString()!;
                    cost = productData.YourCost.ToString()!;
                    imageURL = productData.ImageURL.ToString()!;
                    genderM = productData.Men.ToString()!;
                    genderF = productData.Women.ToString()!;
                    giftSet = productData.GiftSet.ToString()!;
                    minimumQty = GlobalConstants.minimumQuantity;

                    shopifyFixedPrice = applicationState.shopifyFixedPricesList.Where(m => m.Sku == sku && m.EcomStoreId == (int)GlobalConstants.STORENAME.SHOPIFY).FirstOrDefault();

                    if (shopifyFixedPrice != null)
                    {
                        try
                        {
                            if (Convert.ToDecimal(shopifyFixedPrice!.FixPrice) > 0)
                            {
                                cost = shopifyFixedPrice.FixPrice!;

                                isFixedPrice = true;
                            }
                        }
                        catch (Exception)
                        {
                            isFixedPrice = false;
                        }
                    }

                    if (isFixedPrice)
                    {
                        updatedCost = cost;
                    }
                    else
                    {
                        updatedCost = Convert.ToString(applicationState.CalculateMarkupPrice(Convert.ToDecimal(cost), GlobalConstants.STORENAME.SHOPIFY));
                    }

                    if (!ValidateRestrictedSKU(sku))
                    {
                        continue;
                    }

                    if (genderM == "Y" && genderF == "N")
                    {
                        genderDescription = "Men";
                    }
                    else if (genderM == "N" && genderF == "Y")
                    {
                        genderDescription = "Women";
                    }
                    else
                    {
                        genderDescription = "Unisex";
                    }

                    if (productData.Name.Split(',').Length > 1)
                    {
                        weightDescription = productData.Name.Split(",")[1];
                    }
                    else
                    {
                        weightDescription = variantTitle;
                    }

                    if (weightDescription.ToUpper().Contains("CANDLE"))
                    {
                        weightDescription = "Candle - " + weightDescription;
                    }

                    if (weightDescription.ToUpper().Contains("MAKEUP") ||
                        weightDescription.ToUpper().Contains("LIPSTICK") ||
                        weightDescription.ToUpper().Contains("FOUNDATION") ||
                        weightDescription.ToUpper().Contains("CONCEALER") ||
                        weightDescription.ToUpper().Contains("EYE") ||
                        weightDescription.ToUpper().Contains("MASCARA") ||
                        weightDescription.ToUpper().Contains("POWDER") ||
                        weightDescription.ToUpper().Contains("SERUM") ||
                        weightDescription.ToUpper().Contains("CLEANSING") ||
                        weightDescription.ToUpper().Contains("SKIN")
                        )
                    {
                        weightDescription = "Makeup - " + weightDescription;
                    }

                    if (giftSet == "Y")
                    {
                        weightDescription = "Gift Set - " + weightDescription;
                    }

                    //currentProduct = productsRepository.GetAll().Where(m => m.Sku == sku).ToList().FirstOrDefault();

                    if (currentProduct != null)
                    {
                        if (currentProduct.SkuPrefix?.ToUpper() == GlobalConstants.fragranceXSKUPrefix.ToUpper())
                        {
                            ProductsRepository productsContext = new();
                            OverrideVariantUpdateModel overrideVariantUpdateModel = new();
                            OverrideVariantImageUpdateModel overrideVariantImageUpdateModel = new();
                            NewVariantImageResponseModel newImage = new();
                            long[] variantIds = new long[] { Convert.ToInt64(currentProduct.VariantId!) };

                            try
                            {

                                overrideVariantUpdateModel.variant.id = Convert.ToInt64(currentProduct.VariantId!);
                                overrideVariantUpdateModel.variant.sku = fullSku;
                                overrideVariantUpdateModel.variant.price = updatedCost;
                                overrideVariantUpdateModel.variant.title = variantTitle;
                                overrideVariantUpdateModel.variant.option1 = weightDescription;

                                shopifyAPI.OverrideShopifyVariant(overrideVariantUpdateModel);

                                overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(currentProduct.ShopifyId);
                                overrideVariantImageUpdateModel.image.src = productData.ImageURL;
                                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                shopifyAPI.DeleteProductVariantImage(Convert.ToInt64(currentProduct.ShopifyId), Convert.ToInt64(currentProduct.ImageId));
                                newImage = shopifyAPI.CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                currentProduct.SkuPrefix = skuPrefix;
                                currentProduct.ImageId = newImage.image.id.ToString();

                                isSKUReplaced = true;

                                productsContext.Update(currentProduct);

                                productsContext.Save();
                            }
                            catch (Exception ex)
                            {
                                applicationState.LogErrorToFile(ex);
                            }
                        }
                    }

                    if (!isSKUReplaced)
                    {
                        if (currentProduct == null)
                        {
                            Variant variant = new();
                            ShopifyInventoryDatum? sameNameProduct = new();

                            variant.title = variantTitle;
                            variant.sku = fullSku;
                            variant.barcode = sku.Trim();
                            variant.price = updatedCost;
                            variant.weight = Convert.ToDouble(weight);
                            variant.inventory_management = "shopify";
                            variant.inventory_policy = "deny";
                            variant.fulfillment_service = "manual";
                            variant.inventory_quantity = Convert.ToInt32(minimumQty);
                            variant.option1 = weightDescription;
                            variant.option2 = genderDescription;
                            variant.requires_shipping = GlobalConstants.requiresShipping;

                            image.src = imageURL;

                            if (!WeightOption.values.Contains("weightDescription"))
                            {
                                WeightOption.values.Add(weightDescription);
                            }

                            if (!WeightOption.values.Contains("genderDescription"))
                            {
                                GenderOption.values.Add(genderDescription);
                            }

                            //sameNameProduct = productsRepository.GetAll().Where(m => m.ProductName == mainTitle && m.ProductGender == genderDescription).FirstOrDefault();

                            if (sameNameProduct != null)
                            {
                                NewVariantRootModel newVariantRootModel = new();
                                NewVariantMerge newVariantMerge = new();
                                OverrideVariantImageUpdateModel overrideVariantImageUpdateModel = new();
                                NewVariantImageResponseModel newImage = new();
                                ShopifyInventoryDatum shopifyInventoryDatum = new();
                                long[] variantIds;
                                NewVariantRequest newVariantRequest = new();
                                ProductsRepository productsRepositoryVariant = new();

                                try
                                {
                                    newVariantRequest.title = variant.title;
                                    newVariantRequest.sku = variant.sku;
                                    newVariantRequest.barcode = variant.barcode;
                                    newVariantRequest.price = variant.price;
                                    newVariantRequest.weight = variant.weight;
                                    newVariantRequest.inventory_management = variant.inventory_management;
                                    newVariantRequest.inventory_policy = variant.inventory_policy;
                                    newVariantRequest.fulfillment_service = variant.fulfillment_service;
                                    newVariantRequest.option1 = variant.option1;
                                    newVariantRequest.option2 = variant.option2;
                                    newVariantRequest.requires_shipping = GlobalConstants.requiresShipping;

                                    newVariantMerge.variant = newVariantRequest;

                                    newVariantRootModel = shopifyAPI.CreateNewVariant(newVariantMerge, sameNameProduct.ShopifyId!.ToString());

                                    variantIds = new long[] { Convert.ToInt64(newVariantRootModel.variant.id) };

                                    overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(sameNameProduct.ShopifyId);
                                    overrideVariantImageUpdateModel.image.src = imageURL;
                                    overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                    newImage = shopifyAPI.CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                    shopifyInventoryDatum.ProductName = mainTitle.Split(",")[0];
                                    shopifyInventoryDatum.ProductGender = genderDescription;
                                    shopifyInventoryDatum.ShopifyId = sameNameProduct.ShopifyId.ToString();
                                    shopifyInventoryDatum.BrandName = sameNameProduct.BrandName;
                                    shopifyInventoryDatum.Sku = sku.Trim();
                                    shopifyInventoryDatum.SkuPrefix = skuPrefix;
                                    shopifyInventoryDatum.VariantId = newVariantRootModel.variant.id.ToString();
                                    shopifyInventoryDatum.InventoryItemId = newVariantRootModel.variant.inventory_item_id.ToString();
                                    shopifyInventoryDatum.ImageId = newImage.image.id.ToString();

                                    productsRepositoryVariant.Insert(shopifyInventoryDatum);

                                    productsRepositoryVariant.Save();

                                    UpdateProductStockQuantity(sku, Convert.ToInt32(GlobalConstants.minimumQuantity));
                                }
                                catch (Exception ex)
                                {
                                    applicationState.LogErrorToFile(ex);
                                }
                            }
                            else
                            {
                                shopifyProductModelData.product.variants.Add(variant);
                                shopifyProductModelData.product.images.Add(image);

                                isVariantAdded = true;
                            }

                            applicationState.AddMessageToLogs(fullSku + " : SKU merged");
                        }
                        else if (currentProduct.IsOutOfStock)
                        {
                            UpdateProductStockQuantity(sku, Convert.ToInt32(GlobalConstants.minimumQuantity));
                        }

                        if (currentProduct != null)
                        {
                            UpdateProductNewPrice(sku, currentProduct.VariantId!.ToString(), Convert.ToDecimal(updatedCost));
                        }
                    }
                }

                if (isVariantAdded)
                {
                    shopifyProductModelData.product.options.Add(WeightOption);
                    shopifyProductModelData.product.options.Add(GenderOption);

                    shopifyProductResponseData = shopifyAPI.CreateNewShopifyItem(shopifyProductModelData);

                    if (shopifyProductResponseData.product.variants.Count > 0)
                    {
                        List<ProductImageAttachVarient> productImageAttachVarientsList = new();

                        productImageAttachVarientsList = UpdateProductVarientImages(shopifyProductResponseData, productsToProcessData);

                        foreach (Variant productVarient in shopifyProductResponseData.product.variants)
                        {
                            ShopifyInventoryDatum shopifyInventoryDatum = new();

                            shopifyInventoryDatum.ProductName = mainTitle;
                            shopifyInventoryDatum.ProductGender = genderDescription;
                            shopifyInventoryDatum.ShopifyId = shopifyProductResponseData.product.id.ToString();
                            shopifyInventoryDatum.BrandName = shopifyProductResponseData.product.vendor;
                            shopifyInventoryDatum.Sku = productVarient.sku.Substring(skuPrefix.Length, productVarient.sku.Length - skuPrefix.Length);
                            shopifyInventoryDatum.SkuPrefix = skuPrefix;
                            shopifyInventoryDatum.VariantId = productVarient.id.ToString();
                            shopifyInventoryDatum.InventoryItemId = productVarient.inventory_item_id.ToString();

                            try
                            {
                                shopifyInventoryDatum.ImageId = productImageAttachVarientsList.Where(m => m.image.variant_ids.Contains(productVarient.id)).FirstOrDefault()!.image.id.ToString();
                            }
                            catch (Exception ex)
                            {
                                applicationState.LogErrorToFile(ex);
                            }

                            shopifyInventoryDataList.Add(shopifyInventoryDatum);
                        }

                        if (shopifyInventoryDataList.Count > 0)
                        {
                            productsRepositoryContext.InsertMultiple(shopifyInventoryDataList);

                            productsRepositoryContext.Save();

                            applicationState.AddMessageToLogs(Convert.ToString("--------New product created successfully--------"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        public void ProcessProductToWalmart(string walmartProductsTextData, GlobalConstants.WALMARTFEEDTYPE wALMARTFEEDTYPE)
        {
            string response = string.Empty;

            try
            {
                response = walmartAPI.PostProductsToWalmart(walmartProductsTextData, wALMARTFEEDTYPE);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        public void ProcessRetiredProductToWalmart(string sku)
        {
            string response = string.Empty;

            try
            {
                response = walmartAPI.RetireWalmartFeed(sku);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private List<ProductImageAttachVarient> UpdateProductVarientImages(ShopifyProductModel shopifyProductModelData, ThePerfumeSpotProductsList csvProductsToProcessModel)
        {
            ShopifyProductModel shopifyProductData = new();
            List<ProductImageAttachVarient> productImageAttachVarientsList = new();

            try
            {
                shopifyProductData = shopifyProductModelData;

                foreach (Variant productVariant in shopifyProductData.product.variants)
                {
                    ProductImageAttachVarient productImageAttachVarient = new();
                    Image1 image1 = new();
                    string imageURL = csvProductsToProcessModel.products.Where(m => m.UPC == productVariant.sku.Substring(GlobalConstants.tpsSKUPrefix.Length, productVariant.sku.Length - GlobalConstants.tpsSKUPrefix.Length)).First().ImageURL;
                    string[] imageURLParts = imageURL.Split('/');
                    string imageName = imageURLParts[imageURLParts.Length - 1];
                    long[] variantIds = new long[] { productVariant.id };

                    imageName = imageName.Substring(0, imageName.Length - 4);

                    productImageAttachVarient.image.id = shopifyProductData.product.images.Where(m => m.src.Contains(imageName)).First().id;
                    productImageAttachVarient.image.variant_ids = variantIds;

                    shopifyAPI.UpdateProductVariantImage(productImageAttachVarient, productVariant.product_id);

                    productImageAttachVarientsList.Add(productImageAttachVarient);
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return productImageAttachVarientsList;
        }

        public void UpdateProductStockQuantity(string sku, int quantity)
        {
            ShopifyProductInventoryModel shopifyProductInventoryData = new();
            ShopifyInventoryDatum ShopifyInventoryData = new();
            string shopifyID = string.Empty;
            string vendor = string.Empty;
            string inventoryItemId = string.Empty;
            List<string> restrictedSKus = new();

            try
            {
                //ShopifyInventoryData = productsRepository.GetAll().Where(m => m.Sku == sku).First();

                vendor = ShopifyInventoryData.BrandName!;

                if (!ValidateRestrictedBrand(vendor, new List<string> { sku }))
                {
                    return;
                }

                if (!ValidateRestrictedSKU(sku))
                {
                    return;
                }

                shopifyID = ShopifyInventoryData.ShopifyId!;
                inventoryItemId = ShopifyInventoryData.InventoryItemId!;

                if (GlobalConstants.markOutOfStock.ToUpper() == "Y")
                {
                    shopifyProductInventoryData.inventory_item_id = Convert.ToInt64(inventoryItemId);
                    shopifyProductInventoryData.location_id = Convert.ToInt64(GlobalConstants.locationId);
                    shopifyProductInventoryData.available = quantity;

                    shopifyAPI.SetProductInventoryLevel(shopifyProductInventoryData);

                    if (quantity <= 0)
                    {
                        applicationState.AddMessageToLogs(Convert.ToString(sku + " : Marked out of stock"));

                        MarkProductOutOfStock(sku, true);
                    }
                    else
                    {
                        applicationState.AddMessageToLogs(Convert.ToString(sku + " : Quantity Updated - " + quantity.ToString()));

                        MarkProductOutOfStock(sku, false);
                    }
                }
                else
                {
                    applicationState.AddMessageToLogs(Convert.ToString(sku + " : Modification is not allowed"));
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private bool MarkProductOutOfStock(string sku, bool isOutOfStock)
        {
            ShopifyInventoryDatum ShopifyInventoryData = new();
            ProductsRepository productsRepository = new();
            bool result = false;

            try
            {
                ShopifyInventoryData = productsRepository.GetAll().Where(m => m.Sku == sku).First();

                ShopifyInventoryData.IsOutOfStock = isOutOfStock;

                productsRepository.Update(ShopifyInventoryData);

                productsRepository.Save();

                result = true;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        private void UpdateProductNewPrice(string sku, string variantID, decimal updatedPrice)
        {
            SingleVariantPriceUpdate singleVariantPriceUpdate = new();

            try
            {
                singleVariantPriceUpdate.variant.id = Convert.ToInt64(variantID);
                singleVariantPriceUpdate.variant.price = Convert.ToString(updatedPrice);

                if (singleVariantPriceUpdate != null)
                {
                    shopifyAPI.UpdateVariantPrice(singleVariantPriceUpdate);
                }

                applicationState.AddMessageToLogs(Convert.ToString(sku + " : Price Updated to : " + updatedPrice.ToString()));
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private void UpdateSKUAsRestricted(List<string> sku)
        {
            ProductsRepository productsRepository = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new();

            try
            {
                shopifyInventoryDataList = productsRepository.GetAll().Where(m => sku.Contains(m.Sku!)).ToList();

                foreach (ShopifyInventoryDatum product in shopifyInventoryDataList)
                {
                    try
                    {
                        if (shopifyAPI.DeleteShopifyProduct(product))
                        {
                            productsRepository.DeleteMultiple(productsRepository.GetAll().Where(m => m.ShopifyId == product.ShopifyId).ToList());
                        }
                    }
                    catch (Exception ex)
                    {
                        applicationState.LogErrorToFile(ex);
                    }
                }

                productsRepository.Save();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private bool ValidateRestrictedBrand(string vendor, List<string> restrictedSKus)
        {
            bool result = true;

            try
            {
                if ((from s in restrictedBrandsRepository.GetAll()
                     where (s.ApiType == "ALL" || s.ApiType == "SBB") && s.BrandName == vendor && s.EcomStoreId == (int)GlobalConstants.STORENAME.WALMART
                     select s).ToList<RestrictedBrand>().Count > 0)
                {
                    applicationState.AddMessageToLogs(Convert.ToString(vendor + " : Restricted Brand Found"));

                    UpdateSKUAsRestricted(restrictedSKus);

                    result = false;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        private bool ValidateRestrictedSKU(string sku)
        {
            bool result = true;
            List<string> restrictedSKus = new();

            try
            {
                if ((from s in restrictedSkusRepository.GetAll()
                     where (s.ApiType == "ALL" || s.ApiType == "SBB") && s.Sku == sku && s.EcomStoreId == (int)GlobalConstants.STORENAME.SHOPIFY
                     select s).ToList<RestrictedSku>().Count > 0)
                {
                    applicationState.AddMessageToLogs(Convert.ToString(sku + " : Restricted SKU Found"));

                    restrictedSKus.Clear();

                    restrictedSKus.Add(sku);

                    UpdateSKUAsRestricted(restrictedSKus);

                    result = false;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return result;
        }

        private decimal CalculateMarkedUpPrice(string sku, string cost)
        {
            FixedPrice? walmartFixedPrice = new();
            bool isFixedPrice = false;
            decimal updatedCost = 0;

            try
            {
                walmartFixedPrice = applicationState.shopifyFixedPricesList.Where(m => m.Sku == sku && m.EcomStoreId == (int)GlobalConstants.STORENAME.WALMART).FirstOrDefault();

                if (walmartFixedPrice != null)
                {
                    try
                    {
                        if (Convert.ToDecimal(walmartFixedPrice!.FixPrice) > 0)
                        {
                            cost = walmartFixedPrice.FixPrice!;

                            isFixedPrice = true;
                        }
                    }
                    catch (Exception)
                    {
                        isFixedPrice = false;
                    }
                }

                if (isFixedPrice)
                {
                    updatedCost = Convert.ToDecimal(cost);
                }
                else
                {
                    updatedCost = applicationState.CalculateMarkupPrice(Convert.ToDecimal(cost), GlobalConstants.STORENAME.WALMART);
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return updatedCost;
        }
    }
}
