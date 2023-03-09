using CsvHelper;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Globalization;
using System.Text;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Shopify
{
    internal class ShopifyFragranceNet
    {
        ApplicationState applicationState;

        private readonly IProductsRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly ShopifyAPI shopifyAPI;

        public ShopifyFragranceNet()
        {
            applicationState = ApplicationState.GetState;
            shopifyAPI = new();
            productsRepository = new ProductsRepository();
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

        public FragranceNetProductsList GetDataFromSource()
        {
            string fileTextData;
            string csvTextData;
            FragranceNetProductsList fragranceNetProducts = new();
            byte[] csvBytes;

            try
            {
                fileTextData = FetchDataFromAPI();

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
                        fragranceNetProducts.products = csv.GetRecords<FragranceNetProduct>().Where(m => m.designer != "").ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceNetProducts;
        }

        public List<ShopifyInventoryDatum> FilterRemovedProducts(FragranceNetProductsList fragranceNetProducts)
        {
            List<ShopifyInventoryDatum> shopifyProductsToRemove = new();
            List<FragranceNetProduct> products = new();

            try
            {
                products = fragranceNetProducts.products;

                shopifyProductsToRemove = (from s in productsRepository.GetBySkuPrefix(FRAGRANCENETSKUPREFIX)
                                           where !products.Any(x => x.sku == s.Sku && s.SkuPrefix == FRAGRANCENETSKUPREFIX)
                                           select s).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return shopifyProductsToRemove;
        }

        public List<FragranceNetProductsList> FormatSourceProductsData(FragranceNetProductsList fragranceNetProducts)
        {
            List<FragranceNetProduct> productsListPrePrepare = new();
            List<FragranceNetProduct> productsListPostPrepare = new();
            List<FragranceNetProductsList> productsDataListPrepared = new();
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

                    productsListPostPrepare = productsListPrePrepare.Where(m => m.name == productName && m.gender == gender).ToList();

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
            ProductsRepository productsRepositoryContext = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new();
            ShopifyProductModel shopifyProductModelData = new();
            ShopifyProductModel shopifyProductResponseData = new();

            try
            {
                headerProduct = productsToProcessData.products[0];

                skuPrefix = FRAGRANCENETSKUPREFIX;

                mainTitle = headerProduct.name;
                vendor = headerProduct.designer;
                productDescription = headerProduct.productDescription + " " + headerProduct.fragranceNotes + " " + headerProduct.recommendedUse;

                restrictedSKus = productsToProcessData.products.Select(m => m.sku).ToList();

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

                foreach (FragranceNetProduct productData in productsToProcessData.products)
                {
                    ShopifyInventoryDatum? currentProduct = new();
                    FixedPrice? shopifyFixedPrice = new();
                    Image1 image = new();
                    bool isFixedPrice = false;
                    bool isSKUReplaced = false;
                    string cost = string.Empty;
                    string updatedCost = string.Empty;
                    string variantTitle = string.Empty;

                    variantTitle = productData.productDescription;
                    weightDescription = productData.productDescription + " " + productData.productCategory + " " + productData.itemType;
                    sku = productData.sku.ToString()!;
                    fullSku = skuPrefix + sku.Trim();
                    weight = "0";
                    cost = productData.fnetWholesalePrice.ToString()!;
                    imageURL = productData.imageLarge.ToString()!;
                    gender = productData.gender.ToString()!;
                    minimumQty = MINIMUMQUANTITY;

                    shopifyFixedPrice = applicationState.shopifyFixedPricesList.Where(m => m.Sku == sku && m.EcomStoreId == (int)STORENAME.SHOPIFY).FirstOrDefault();

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
                        updatedCost = Convert.ToString(applicationState.CalculateMarkupPrice(Convert.ToDecimal(cost), STORENAME.SHOPIFY));
                    }

                    if (!ValidateRestrictedSKU(sku))
                    {
                        continue;
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

                    currentProduct = productsRepository.GetAll().Where(m => m.Sku == sku).ToList().FirstOrDefault();

                    if (currentProduct != null)
                    {
                        if (currentProduct.SkuPrefix?.ToUpper() == TPSSKUPREFIX.ToUpper() || currentProduct.SkuPrefix?.ToUpper() == FRAGRANCEXSKUPREFIX.ToUpper())
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
                                overrideVariantImageUpdateModel.image.src = productData.imageLarge;
                                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                shopifyAPI.DeleteProductVariantImage(Convert.ToInt64(currentProduct.ShopifyId), Convert.ToInt64(currentProduct.ImageId));
                                newImage = shopifyAPI.CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                currentProduct.SkuPrefix = FRAGRANCENETSKUPREFIX;
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

                                    productsRepositoryVariant.Insert(shopifyInventoryDatum);

                                    productsRepositoryVariant.Save();

                                    UpdateProductStockQuantity(sku, Convert.ToInt32(MINIMUMQUANTITY));
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
                            UpdateProductStockQuantity(sku, Convert.ToInt32(MINIMUMQUANTITY));
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

        private List<ProductImageAttachVarient> UpdateProductVarientImages(ShopifyProductModel shopifyProductModelData, FragranceNetProductsList csvProductsToProcessModel)
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
                    string imageURL = csvProductsToProcessModel.products.Where(m => m.sku == productVariant.sku.Substring(FRAGRANCENETSKUPREFIX.Length, productVariant.sku.Length - FRAGRANCENETSKUPREFIX.Length)).First().imageLarge;
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
                ShopifyInventoryData = productsRepository.GetAll().Where(m => m.Sku == sku).First();

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
                     where (s.ApiType == "ALL" || s.ApiType == "SBC") && s.BrandName == vendor && s.EcomStoreId == (int)STORENAME.SHOPIFY
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
                     where (s.ApiType == "ALL" || s.ApiType == "SBC") && s.Sku == sku && s.EcomStoreId == (int)STORENAME.SHOPIFY
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
