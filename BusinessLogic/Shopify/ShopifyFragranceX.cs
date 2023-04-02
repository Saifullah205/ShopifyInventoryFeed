using ShopifyInventorySync.BusinessLogic.Vendors;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Shopify
{
    internal class ShopifyFragranceX
    {
        ApplicationState applicationState;

        private readonly IProductsRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly ShopifyAPI shopifyAPI;

        public ShopifyFragranceX()
        {
            applicationState = ApplicationState.GetState;
            shopifyAPI = new();
            productsRepository = new ProductsRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();
        }

        public List<ShopifyInventoryDatum> FilterRemovedProducts(FragranceXProductsList fragranceXProductsList)
        {
            List<ShopifyInventoryDatum> shopifyProductsToRemove = new();
            List<FragranceXProduct> products = new();

            try
            {
                products = fragranceXProductsList.products;

                shopifyProductsToRemove = (from s in productsRepository.GetBySkuPrefix(FRAGRANCEXSKUPREFIX)
                                           where !products.Any(x => x.Upc == s.Sku)
                                           select s).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return shopifyProductsToRemove;
        }

        public List<FragranceXProductsList> FormatSourceProductsData(FragranceXProductsList fragranceXProducts)
        {
            List<FragranceXProduct> productsListPrePrepare = new();
            List<FragranceXProduct> productsListPostPrepare = new();
            List<FragranceXProductsList> productsDataListPrepared = new();
            FragranceXProduct selectedProduct = new();
            string productName = string.Empty;
            string gender = string.Empty;

            try
            {
                productsDataListPrepared.Clear();

                productsListPrePrepare = fragranceXProducts.products;

                while (productsListPrePrepare.Count > 0)
                {
                    FragranceXProductsList csvProductsToProcessModel = new();

                    selectedProduct = productsListPrePrepare.First();
                    productName = selectedProduct.ProductName;
                    gender = selectedProduct.Gender;

                    productsListPostPrepare = productsListPrePrepare.Where(m => m.ProductName == productName && m.Gender == gender).ToList();

                    csvProductsToProcessModel.products = productsListPostPrepare;

                    productsDataListPrepared.Add(csvProductsToProcessModel);

                    productsListPrePrepare.RemoveAll(m => m.ProductName == productName && m.Gender == gender);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsDataListPrepared;
        }

        public void ProcessProductToShopify(FragranceXProductsList productsToProcessData)
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
            FragranceXProduct headerProduct = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new();
            ShopifyProductModel shopifyProductModelData = new();
            ShopifyProductModel shopifyProductResponseData = new();

            try
            {
                headerProduct = productsToProcessData.products[0];

                skuPrefix = FRAGRANCEXSKUPREFIX;

                mainTitle = headerProduct.ProductName;
                vendor = headerProduct.BrandName;
                productDescription = headerProduct.Description;

                mainTitle = mainTitle + " by " + vendor;

                restrictedSKus = productsToProcessData.products.Select(m => m.Upc).ToList();

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

                foreach (FragranceXProduct productData in productsToProcessData.products)
                {
                    ShopifyInventoryDatum? currentProduct = new();
                    FixedPrice? shopifyFixedPrice = new();
                    Image1 image = new();
                    bool isSKUReplaced = false;
                    string cost = string.Empty;
                    string updatedCost = string.Empty;

                    variantTitle = productData.ProductName + " by " + productData.BrandName;
                    weightDescription = productData.Size + " " + productData.Type;
                    sku = productData.Upc.ToString()!;
                    fullSku = skuPrefix + sku.Trim();
                    weight = "0";
                    cost = productData.WholesalePriceUSD.ToString()!;
                    imageURL = productData.LargeImageUrl.ToString()!;
                    gender = productData.Gender.ToString()!;
                    minimumQty = productData.QuantityAvailable.ToString();
                    updatedCost = applicationState.GetMarkedUpPrice(sku, productData.WholesalePriceUSD.ToString(), STORENAME.SHOPIFY).ToString();

                    if (!ValidateRestrictedSKU(sku))
                    {
                        continue;
                    }

                    if (productData.Type.ToUpper().Contains("UNISEX") || productData.Description.ToUpper().Contains("UNISEX"))
                    {
                        gender = "UNISEX";
                    }

                    if (gender.ToUpper() == "MEN")
                    {
                        genderDescription = "Men";
                    }
                    else if (gender.ToUpper() == "WOMEN")
                    {
                        genderDescription = "Women";
                    }
                    else if (gender.ToUpper() == "UNISEX")
                    {
                        genderDescription = "Unisex";
                    }

                    if (giftSet == "Y")
                    {
                        variantTitle = "Gift Set - " + variantTitle;
                    }

                    currentProduct = productsRepository.GetAll().Where(m => m.Sku == sku).ToList().FirstOrDefault();

                    if (currentProduct != null)
                    {
                        if (currentProduct.SkuPrefix?.ToUpper() != FRAGRANCEXSKUPREFIX.ToUpper() && currentProduct.Price > Convert.ToDecimal(productData.WholesalePriceUSD))
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
                                overrideVariantImageUpdateModel.image.src = productData.LargeImageUrl;
                                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                shopifyAPI.DeleteProductVariantImage(Convert.ToInt64(currentProduct.ShopifyId), Convert.ToInt64(currentProduct.ImageId));
                                newImage = shopifyAPI.CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                currentProduct.SkuPrefix = FRAGRANCEXSKUPREFIX;
                                currentProduct.ImageId = newImage.image.id.ToString();
                                currentProduct.Price = Convert.ToDecimal(productData.WholesalePriceUSD);

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
                            ShopifyInventoryDatum? sameSKUProduct = new();

                            variant.title = variantTitle;
                            variant.sku = fullSku;
                            variant.barcode = sku.Trim();
                            variant.price = updatedCost;
                            variant.weight = Convert.ToDouble(weight);
                            variant.inventory_management = "shopify";
                            variant.inventory_policy = "deny";
                            variant.fulfillment_service = "manual";
                            variant.inventory_quantity = productData.QuantityAvailable;
                            variant.option1 = weightDescription;
                            variant.option2 = genderDescription;
                            variant.requires_shipping = REQUIRESSHIPPING;

                            image.src = imageURL;

                            if (!WeightOption.values.Contains("weightDescription"))
                            {
                                WeightOption.values.Add(weightDescription);
                            }

                            if (!WeightOption.values.Contains("genderDescription"))
                            {
                                GenderOption.values.Add(genderDescription);
                            }

                            sameNameProduct = productsRepository.GetAll().Where(m => m.ProductName == variantTitle && m.ProductGender == genderDescription).FirstOrDefault();

                            if (sameNameProduct != null)
                            {
                                sameSKUProduct = productsRepository.GetAll().Where(m => m.ShopifyId == sameNameProduct.ShopifyId && m.Sku == sku).FirstOrDefault();

                                if (sameSKUProduct != null)
                                {
                                    return;
                                }

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
                                    newVariantRequest.requires_shipping = REQUIRESSHIPPING;

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
                                    shopifyInventoryDatum.Price = Convert.ToDecimal(productData.WholesalePriceUSD);

                                    productsRepositoryVariant.Insert(shopifyInventoryDatum);

                                    productsRepositoryVariant.Save();

                                    UpdateProductStockQuantity(sku, productData.QuantityAvailable);
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

                        if (currentProduct != null && currentProduct.SkuPrefix == FRAGRANCEXSKUPREFIX)
                        {
                            ShopifyInventoryDatum shopifyInventoryDatum = new();
                            ProductsRepository productsRepositoryContext = new();
                            shopifyInventoryDatum = productsRepositoryContext.GetById(currentProduct.ShopifyId!);
                            shopifyInventoryDatum.Price = Convert.ToDecimal(productData.WholesalePriceUSD);
                            shopifyInventoryDatum.IsOutOfStock = false;
                            productsRepositoryContext.Update(shopifyInventoryDatum);
                            productsRepositoryContext.Save();

                            UpdateProductNewPrice(sku, currentProduct.VariantId!.ToString(), Convert.ToDecimal(updatedCost));
                            UpdateProductStockQuantity(sku, productData.QuantityAvailable);
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
                            shopifyInventoryDatum.Price = Convert.ToDecimal(productsToProcessData.products.Where(m => m.Upc == shopifyInventoryDatum.Sku).First().WholesalePriceUSD);

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
                            ProductsRepository productsRepositoryContext = new();
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

        private List<ProductImageAttachVarient> UpdateProductVarientImages(ShopifyProductModel shopifyProductModelData, FragranceXProductsList csvProductsToProcessModel)
        {
            ShopifyProductModel shopifyProductData = new();
            List<ProductImageAttachVarient> productImageAttachVarientsList = new();

            try
            {
                shopifyProductData = shopifyProductModelData;

                foreach (Variant productVariant in shopifyProductData.product.variants)
                {
                    try
                    {
                        ProductImageAttachVarient productImageAttachVarient = new();
                        Image1 image1 = new Image1();
                        string imageURL = csvProductsToProcessModel.products.Where(m => m.Upc == productVariant.sku.Substring(FRAGRANCEXSKUPREFIX.Length, productVariant.sku.Length - FRAGRANCEXSKUPREFIX.Length)).FirstOrDefault()!.LargeImageUrl;
                        string[] imageURLParts = imageURL.Split('/');
                        string imageName = imageURLParts[imageURLParts.Length - 1];
                        long[] variantIds = new long[] { productVariant.id };

                        imageName = imageName.Substring(0, imageName.Length - 4);

                        productImageAttachVarient.image.id = shopifyProductData.product.images.Where(m => m.src.Contains(imageName)).First().id;
                        productImageAttachVarient.image.variant_ids = variantIds;

                        shopifyAPI.UpdateProductVariantImage(productImageAttachVarient, productVariant.product_id);

                        productImageAttachVarientsList.Add(productImageAttachVarient);
                    }
                    catch (Exception ex)
                    {
                        applicationState.LogErrorToFile(ex);
                    }
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
                ShopifyInventoryData = productsRepository.GetAll().Where(m => m.Sku == sku && m.SkuPrefix == FRAGRANCEXSKUPREFIX).First();

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

                if (MARKOUTOFSTOCK.ToUpper() == "Y")
                {
                    shopifyProductInventoryData.inventory_item_id = Convert.ToInt64(inventoryItemId);
                    shopifyProductInventoryData.location_id = Convert.ToInt64(LOCATIONID);
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
                ShopifyInventoryData = productsRepository.GetAll().Where(m => m.Sku == sku && m.SkuPrefix == FRAGRANCEXSKUPREFIX).First();

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
                     where (s.ApiType == "ALL" || s.ApiType == FRAGRANCEXSKUPREFIX) && s.BrandName == vendor && s.EcomStoreId == (int)STORENAME.SHOPIFY
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
                     where (s.ApiType == "ALL" || s.ApiType == FRAGRANCEXSKUPREFIX) && s.Sku == sku && s.EcomStoreId == (int)STORENAME.SHOPIFY
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
    }
}
