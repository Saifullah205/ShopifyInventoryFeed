using CsvHelper;
using RestSharp;
using ShopifyInventorySync.Models;
using System.Data;
using System.Globalization;
using System.Reflection;
using static ShopifyInventorySync.ProductsCSVModel;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace ShopifyInventorySync
{
    public partial class ProcessCSVForm : Form
    {
        DataTable productsDataTable = new();
        string selectedCSVFile = string.Empty;
        string defaultLocationID = string.Empty;
        string processingMessages = string.Empty;
        string csvSkuPrefix = String.Empty;
        string fraganceXSkuPrefix = String.Empty;
        int loopThreadWaitIndex = 0;
        int threadsPerSecond = 1;
        int selectedAPI = (int)SharedData.APIType.CSV;
        decimal progressBarTotalValue = 0;
        decimal progressBarIncrementValue = 0;
        decimal progressBarValue = 0;
        List<string> ProductNames = new List<string> ();
        List<ShopifyInventoryDatum> shopifyProductsToRemove = new List<ShopifyInventoryDatum>();
        List<ProductsCSVModel> productsDataList = new List<ProductsCSVModel> ();
        List<ShopifyInventoryDatum> shopifyProductsData = new List<ShopifyInventoryDatum>();
        List<MarkUpPrice> markUpPricesList = new List<MarkUpPrice>();
        List<ApplicationSetting> applicationSettingsList = new List<ApplicationSetting>();
        List<RestrictedBrand> restrictedBrandsList = new ();
        List<RestrictedSku> restrictedSkusList = new();
        List<FragranceXProduct> fragranceXProducts = new();
        List<ShopifyFixedPrice> shopifyFixedPricesList = new();

        public ProcessCSVForm()
        {
            InitializeComponent();

            LoadShopifyProducts();

            txtProcessedProducts.ScrollBars = ScrollBars.Vertical;

            lblSelectedAPI.Text = "CSV API";
        }

        private void LoadShopifyProducts()
        {
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                RefreshShopifySkusList();

                RefreshApplicationSettingsList();

                RefreshMarkUPPricesList();

                RefreshFixedPricesList();

                RefreshRestrictedBrandsList();

                RefreshRestrictedSkusList();

                lblProgressCount.Text = "0% Completed";
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

        }

        private void ClearGridData()
        {
            try
            {
                processingMessages = string.Empty;

                loadedDataGridView.DataSource = null;

                loadedDataGridView.Rows.Clear();

                loadedDataGridView.Refresh();

                if (productsDataTable != null)
                {
                    productsDataTable.Rows.Clear();
                }

                progressBarValue = 0;

                ProcessProgress.Value = (int)progressBarValue;

                lblProgressCount.Text = "0% Completed";
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshShopifySkusList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                shopifyProductsData.Clear();

                shopifyProductsData = shopifyDBContext.ShopifyInventoryData.ToList<ShopifyInventoryDatum>().Where(m => m.IsDisabled == false).ToList<ShopifyInventoryDatum>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshRestrictedSkusList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                restrictedSkusList.Clear();

                restrictedSkusList = shopifyDBContext.RestrictedSkus.ToList<RestrictedSku>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshRestrictedBrandsList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                restrictedBrandsList.Clear();

                restrictedBrandsList = shopifyDBContext.RestrictedBrands.ToList<RestrictedBrand>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshApplicationSettingsList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                applicationSettingsList.Clear();

                applicationSettingsList = shopifyDBContext.ApplicationSettings.ToList<ApplicationSetting>();

                defaultLocationID = GetApplicationSettings("LocationId");
                fraganceXSkuPrefix = GetApplicationSettings("FragranceXSKUPrefix");
                csvSkuPrefix = GetApplicationSettings("ShopifySKUPrefix");
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshMarkUPPricesList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                markUpPricesList.Clear();

                markUpPricesList = shopifyDBContext.MarkUpPrices.ToList<MarkUpPrice>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshFixedPricesList()
        {
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                shopifyFixedPricesList.Clear();

                shopifyFixedPricesList = shopifyDBContext.ShopifyFixedPrices.ToList<ShopifyFixedPrice>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private async void ProcessCSVData(List<CSVProductsToProcessModel> productsToProcessData)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                btnClear.Enabled = false;
                btnProcess.Enabled = false;

                LoadShopifyProducts();

                progressBarTotalValue = productsToProcessData.Count + shopifyProductsToRemove.Count;

                progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                foreach (CSVProductsToProcessModel productsList in productsToProcessData)
                {
                    Task T = Task.Run(() => ProcessProductRow(productsList));

                    tasks.Add(T);

                    if (loopThreadWaitIndex % threadsPerSecond == 0)
                    {
                        await Task.WhenAll(tasks);
                    }

                    loopThreadWaitIndex += 1;

                    IncrementProgressBar();
                }

                foreach (ShopifyInventoryDatum product in shopifyProductsToRemove)
                {
                    Task T = Task.Run(() => UpdateProductStockQuantity(product.Sku!,0));

                    tasks.Add(T);

                    if (loopThreadWaitIndex % threadsPerSecond == 0)
                    {
                        await Task.WhenAll(tasks);
                    }

                    loopThreadWaitIndex += 1;

                    IncrementProgressBar();
                }

                await Task.WhenAll(tasks);

                txtProcessedProducts.Text = processingMessages;

                processingMessages = String.Empty;

                MessageBox.Show("Process Completed Successfully");

                LoadShopifyProducts();

                ClearGridData();

                btnClear.Enabled = true;
                btnProcess.Enabled = true;
            }
            catch (Exception ex)
            {
                btnClear.Enabled = true;

                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }            
        }

        private async void ProcessFragranceXData(List<FragranceXModel> productsToProcessData)
        {
            List<Task> tasks = new List<Task>();

            try
            {
                btnClear.Enabled = false;
                btnProcess.Enabled = false;

                LoadShopifyProducts();

                progressBarTotalValue = productsToProcessData.Count + shopifyProductsToRemove.Count;

                progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                foreach (FragranceXModel productsList in productsToProcessData)
                {
                    Task T = Task.Run(() => ProcessFragranceXProductRow(productsList));

                    tasks.Add(T);

                    if (loopThreadWaitIndex % threadsPerSecond == 0)
                    {
                        await Task.WhenAll(tasks);
                    }

                    loopThreadWaitIndex += 1;

                    IncrementProgressBar();
                }

                foreach (ShopifyInventoryDatum product in shopifyProductsToRemove)
                {
                    Task T = Task.Run(() => UpdateProductStockQuantity(product.Sku!, 0));

                    tasks.Add(T);

                    if (loopThreadWaitIndex % threadsPerSecond == 0)
                    {
                        await Task.WhenAll(tasks);
                    }

                    loopThreadWaitIndex += 1;

                    IncrementProgressBar();
                }

                await Task.WhenAll(tasks);

                txtProcessedProducts.Text = processingMessages;

                processingMessages = String.Empty;

                MessageBox.Show("Process Completed Successfully");

                LoadShopifyProducts();

                ClearGridData();

                btnClear.Enabled = true;
                btnProcess.Enabled = true;
            }
            catch (Exception ex)
            {
                btnClear.Enabled = true;

                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateSKUAsRestricted(List<string> sku)
        {
            ShopifyDbContext shopifyDBContextUpdateProduct = new ShopifyDbContext();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new();

            try
            {
                shopifyInventoryDataList = shopifyDBContextUpdateProduct.ShopifyInventoryData.Where(m=> sku.Contains(m.Sku)).ToList();

                foreach (ShopifyInventoryDatum product in shopifyInventoryDataList)
                {
                    try
                    {
                        if (DeleteShopifyProduct(product))
                        {
                            shopifyDBContextUpdateProduct.ShopifyInventoryData.RemoveRange(shopifyDBContextUpdateProduct.ShopifyInventoryData.Where(m => m.ShopifyId == product.ShopifyId));

                            shopifyDBContextUpdateProduct.SaveChanges();

                            shopifyProductsData.RemoveAll(m => m.ShopifyId == product.ShopifyId);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrorToFile(ex);
                    }                    
                }

                shopifyDBContextUpdateProduct.SaveChanges();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private bool DeleteShopifyProduct(ShopifyInventoryDatum product)
        {
            bool result = false;
            string url = GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products/" + product.ShopifyId + ".json";

            try
            {
                RestClient client = new();
                RestRequest request = new(url,method: Method.Delete);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                RestResponse response = client.Execute(request);

                if (response != null)
                if(response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

            return result;
        }

        private NewVariantRootModel CreateNewVariant(NewVariantMerge newVariantMerge,string shopifyID)
        {
            string url = GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2022-10/products/" + shopifyID.ToString() + "/variants.json";
            NewVariantRootModel newVariantRootModel = new();

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Post);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
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
                LogErrorToFile(ex);
            }

            return newVariantRootModel;
        }

        private bool PostShopifyVariant(OverrideVariantUpdateModel overrideVarianteUpdateModel)
        {
            string url = GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2022-10/variants/" + overrideVarianteUpdateModel.variant.id.ToString() + ".json";
            bool result = false;

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Put);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
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
                LogErrorToFile(ex);
            }

            return result;
        }

        private void ProcessProductRow(CSVProductsToProcessModel productsToProcessData)
        {
            string shopifyID = string.Empty;
            string sku = string.Empty;
            string mainTitle = string.Empty;
            string variantTitle = string.Empty;
            string weight = string.Empty;
            string imageURL = string.Empty;
            string vendor = string.Empty;
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
            bool isGenderFound = false;

            ShopifyDbContext shopifyDBContextUpdateProduct = new ShopifyDbContext();

            List<ShopifyInventoryDatum> shopifyInventoryDataList = new List<ShopifyInventoryDatum>();
            ShopifyProductModel shopifyProductModelData = new ShopifyProductModel();
            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                skuPrefix = csvSkuPrefix;
                minimumQty = GetApplicationSettings("MinimumQuantity");

                mainTitle = productsToProcessData.Products[0].Name.ToString()!;
                vendor = productsToProcessData.Products[0].Brand.ToString()!;

                if (restrictedBrandsList.Where(m => m.BrandName == vendor).ToList<RestrictedBrand>().Count > 0)
                {
                    AddMessageToLogs(Convert.ToString(vendor + " : Restricted Brand Found"));

                    restrictedSKus = (productsToProcessData.Products.Where(m => m.Brand == vendor)).Select(m => m.SKU).ToList<string>();

                    UpdateSKUAsRestricted(restrictedSKus);

                    return;
                }

                shopifyProductModelData.product.title = mainTitle.Split(",")[0];
                shopifyProductModelData.product.body_html = mainTitle.Split(",")[0];
                shopifyProductModelData.product.vendor = vendor;
                shopifyProductModelData.product.status = "active";
                shopifyProductModelData.product.published_at = DateTime.Now;

                GenderOption.name = "Gender";
                GenderOption.position = 2;

                WeightOption.name = "Product";
                WeightOption.position = 1;

                foreach (ProductsCSVModel productData in productsToProcessData.Products)
                {
                    ShopifyFixedPrice shopifyFixedPrice = new(); 
                    ShopifyInventoryDatum currentProduct = new();
                    bool isNewProduct = false;
                    Image1 image = new();
                    isGenderFound = false;
                    genderDescription = string.Empty;
                    bool isFixedPrice = false;
                    string cost = string.Empty;
                    string updatedCost = string.Empty;
                    bool isSKUReplaced = false;
                    string weightDescription = string.Empty;

                    sku = productData.UPC.ToString()!;
                    weight = productData.Weight.ToString()!;
                    cost = productData.YourCost.ToString()!;
                    imageURL = productData.ImageURL.ToString()!;
                    genderM = productData.Men.ToString()!;
                    genderF = productData.Women.ToString()!;
                    giftSet = productData.GiftSet.ToString()!;

                    fullSku = skuPrefix + sku.Trim();                    

                    shopifyFixedPrice = shopifyFixedPricesList.Where(m => m.Sku == sku).ToList<ShopifyFixedPrice>().FirstOrDefault();

                    if (shopifyFixedPrice != null)
                    {
                        try
                        {
                            if (Convert.ToDecimal(shopifyFixedPrice!.FixedPrice) > 0)
                            {
                                cost = shopifyFixedPrice.FixedPrice!;

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
                        updatedCost = Convert.ToString(CalculateMarkupPrice(Convert.ToDecimal(cost)));
                    }

                    if (restrictedSkusList.Where(m => m.Sku == sku).ToList<RestrictedSku>().Count > 0)
                    {
                        AddMessageToLogs(Convert.ToString(fullSku + " : Restricted SKU Found"));

                        restrictedSKus.Clear();

                        restrictedSKus.Add(sku);

                        UpdateSKUAsRestricted(restrictedSKus);

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
                    else if (genderM == "Y" && genderF == "Y")
                    {
                        genderDescription = "Unisex";
                    }

                    if (!string.IsNullOrEmpty(genderDescription))
                    {
                        isGenderFound = true;
                    }

                    variantTitle = productData.Name.Split(',')[0].ToString();

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

                    currentProduct = shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().FirstOrDefault();

                    if (currentProduct != null)
                    {
                        if (currentProduct.SkuPrefix?.ToUpper() == fraganceXSkuPrefix.ToUpper())
                        {
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

                                PostShopifyVariant(overrideVariantUpdateModel);

                                overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(currentProduct.ShopifyId);
                                overrideVariantImageUpdateModel.image.src = productData.ImageURL;
                                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                DeleteProductVariantImage(Convert.ToInt64(currentProduct.ShopifyId), Convert.ToInt64(currentProduct.ImageId));
                                newImage = CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                currentProduct.SkuPrefix = csvSkuPrefix;
                                currentProduct.ImageId = newImage.image.id.ToString();

                                shopifyProductsData.Where(m => m.Sku == sku).FirstOrDefault()!.ImageId = newImage.image.id.ToString();
                                shopifyProductsData.Where(m => m.Sku == sku).FirstOrDefault()!.SkuPrefix = csvSkuPrefix;

                                isSKUReplaced = true;

                                shopifyDBContextUpdateProduct.ShopifyInventoryData.Update(currentProduct);

                                shopifyDBContextUpdateProduct.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                LogErrorToFile(ex);
                            }
                        }
                    }

                    if (!isSKUReplaced)
                    {
                        if (shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().Count <= 0)
                        {
                            Variant variant = new();
                            ShopifyInventoryDatum sameNameProduct = new();

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

                            if (isGenderFound)
                            {
                                variant.option2 = genderDescription;
                            }

                            image.src = imageURL;

                            if (!WeightOption.values.Contains("weightDescription"))
                            {
                                WeightOption.values.Add(weightDescription);
                            }

                            if (!WeightOption.values.Contains("genderDescription"))
                            {
                                GenderOption.values.Add(genderDescription);
                            }

                            sameNameProduct = shopifyProductsData.Where(m => m.ProductName == variantTitle && m.ProductGender == genderDescription).FirstOrDefault()!;

                            if (sameNameProduct != null)
                            {
                                NewVariantRootModel newVariantRootModel = new();
                                NewVariantMerge newVariantMerge = new();
                                OverrideVariantImageUpdateModel overrideVariantImageUpdateModel = new();
                                NewVariantImageResponseModel newImage = new();
                                ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();
                                long[] variantIds;
                                NewVariantRequest newVariantRequest = new();

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

                                    newVariantMerge.variant = newVariantRequest;

                                    newVariantRootModel = CreateNewVariant(newVariantMerge, sameNameProduct.ShopifyId!.ToString());
                                    
                                    variantIds = new long[] { Convert.ToInt64(newVariantRootModel.variant.id) };
                                    
                                    overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(sameNameProduct.ShopifyId);
                                    overrideVariantImageUpdateModel.image.src = productData.ImageURL;
                                    overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                    newImage = CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                    shopifyInventoryDatum.ProductName = mainTitle.Split(",")[0];
                                    shopifyInventoryDatum.ProductGender = genderDescription;
                                    shopifyInventoryDatum.ShopifyId = sameNameProduct.ShopifyId.ToString();
                                    shopifyInventoryDatum.BrandName = sameNameProduct.BrandName;
                                    shopifyInventoryDatum.Sku = sku.Trim();
                                    shopifyInventoryDatum.SkuPrefix = skuPrefix;
                                    shopifyInventoryDatum.VariantId = newVariantRootModel.variant.id.ToString();
                                    shopifyInventoryDatum.InventoryItemId = newVariantRootModel.variant.inventory_item_id.ToString();
                                    shopifyInventoryDatum.ImageId = newImage.image.id.ToString();

                                    shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDatum);

                                    shopifyDBContextUpdateProduct.SaveChanges();

                                    RefreshShopifySkusList();

                                    UpdateProductStockQuantity(sku, 50);
                                }
                                catch (Exception ex)
                                {
                                    LogErrorToFile(ex);
                                }
                            }
                            else
                            {
                                shopifyProductModelData.product.variants.Add(variant);
                                shopifyProductModelData.product.images.Add(image);

                                isVariantAdded = true;
                            }

                            AddMessageToLogs(fullSku + " : SKU merged");

                            isNewProduct = true;
                        }
                        else if (currentProduct!.IsOutOfStock)
                        {
                            UpdateProductStockQuantity(sku, 50);
                        }

                        if (!isNewProduct)
                        {
                            UpdateProductNewPrice(sku, currentProduct!.VariantId!.ToString(), Convert.ToDecimal(updatedCost));
                        }
                    }
                }

                if (isVariantAdded)
                {
                    shopifyProductModelData.product.options.Add(WeightOption);

                    if (isGenderFound)
                    {
                        shopifyProductModelData.product.options.Add(GenderOption);
                    }

                    shopifyProductResponseData = CreateNewShopifyItem(shopifyProductModelData);

                    if(shopifyProductResponseData.product.variants.Count > 0)
                    {
                        List<ProductImageAttachVarient> productImageAttachVarientsList = new();

                        productImageAttachVarientsList = UpdateProductVarientImages(shopifyProductResponseData, productsToProcessData);

                        AddMetaField(sku,shopifyProductResponseData.product.id.ToString());

                        foreach (Variant productVarient in shopifyProductResponseData.product.variants)
                        {
                            ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();

                            shopifyInventoryDatum.ProductName = mainTitle.Split(",")[0];
                            shopifyInventoryDatum.ProductGender = genderDescription;
                            shopifyInventoryDatum.ShopifyId = shopifyProductResponseData.product.id.ToString();
                            shopifyInventoryDatum.BrandName = shopifyProductResponseData.product.vendor;
                            shopifyInventoryDatum.Sku = productVarient.sku.Substring(csvSkuPrefix.Length, productVarient.sku.Length - csvSkuPrefix.Length);
                            shopifyInventoryDatum.SkuPrefix = skuPrefix;
                            shopifyInventoryDatum.VariantId = productVarient.id.ToString();
                            shopifyInventoryDatum.InventoryItemId = productVarient.inventory_item_id.ToString();
                            shopifyInventoryDatum.ImageId = productImageAttachVarientsList.Where(m => m.image.variant_ids.Contains(productVarient.id)).FirstOrDefault()!.image.id.ToString();

                            shopifyInventoryDataList.Add(shopifyInventoryDatum);
                        }

                        if(shopifyInventoryDataList.Count > 0)
                        {
                            shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDataList);

                            shopifyDBContextUpdateProduct.SaveChanges();

                            AddMessageToLogs(Convert.ToString("--------New product created successfully--------"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }
        }

        private void ProcessFragranceXProductRow(FragranceXModel productsToProcessData)
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

            ShopifyDbContext shopifyDBContextUpdateProduct = new ShopifyDbContext();

            ShopifyInventoryDatum currentProduct = new();
            List<ShopifyInventoryDatum> shopifyInventoryDataList = new List<ShopifyInventoryDatum>();
            ShopifyProductModel shopifyProductModelData = new ShopifyProductModel();
            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                skuPrefix = GetApplicationSettings("FragranceXSKUPrefix");

                mainTitle = productsToProcessData.products[0].ProductName;
                vendor = productsToProcessData.products[0].BrandName;
                productDescription = productsToProcessData.products[0].Description;

                mainTitle = mainTitle + " by " + vendor;

                if (restrictedBrandsList.Where(m => m.BrandName == vendor).ToList<RestrictedBrand>().Count > 0)
                {
                    AddMessageToLogs(Convert.ToString(vendor + " : Restricted Brand Found"));

                    restrictedSKus = (productsToProcessData.products.Where(m => m.BrandName == vendor)).Select(m => m.Upc).ToList<string>();

                    UpdateSKUAsRestricted(restrictedSKus);

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
                    Image1 image = new();
                    ShopifyFixedPrice shopifyFixedPrice = new();
                    bool isFixedPrice = false;
                    string cost = string.Empty;
                    string updatedCost = string.Empty;

                    sku = productData.Upc.ToString()!;
                    weight = "0";
                    cost = productData.WholesalePriceUSD.ToString()!;
                    imageURL = productData.LargeImageUrl.ToString()!;
                    gender = productData.Gender.ToString()!;
                    minimumQty = productData.QuantityAvailable.ToString();

                    shopifyFixedPrice = shopifyFixedPricesList.Where(m => m.Sku == sku).ToList<ShopifyFixedPrice>().FirstOrDefault();

                    if(shopifyFixedPrice != null)
                    {
                        try
                        {
                            if (Convert.ToDecimal(shopifyFixedPrice!.FixedPrice) > 0)
                            {
                                cost = shopifyFixedPrice.FixedPrice!;

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
                        updatedCost = Convert.ToString(CalculateMarkupPrice(Convert.ToDecimal(cost)));
                    }

                    if (productData.Type.ToUpper().Contains("UNISEX") || productData.Description.ToUpper().Contains("UNISEX"))
                    {
                        gender = "UNISEX";
                    }

                    fullSku = skuPrefix + sku.Trim();

                    currentProduct = shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().FirstOrDefault();

                    if (restrictedSkusList.Where(m => m.Sku == sku).ToList<RestrictedSku>().Count > 0)
                    {
                        AddMessageToLogs(Convert.ToString(fullSku + " : Restricted SKU Found"));

                        restrictedSKus.Clear();

                        restrictedSKus.Add(sku);

                        UpdateSKUAsRestricted(restrictedSKus);

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

                    if (shopifyProductsData.Where(m => m.Sku == sku).ToList<ShopifyInventoryDatum>().Count <= 0)
                    {
                        Variant variant = new();
                        ShopifyInventoryDatum sameNameProduct = new();

                        variantTitle = productData.ProductName + " by " + productData.BrandName;
                        weightDescription = productData.Size + " " + productData.Type;

                        if (giftSet == "Y")
                        {
                            variantTitle = "Gift Set - " + variantTitle;
                        }

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

                        image.src = imageURL;

                        if (!WeightOption.values.Contains("weightDescription"))
                        {
                            WeightOption.values.Add(weightDescription);
                        }

                        if (!WeightOption.values.Contains("genderDescription"))
                        {
                            GenderOption.values.Add(genderDescription);
                        }

                        sameNameProduct = shopifyProductsData.Where(m => m.ProductName == variantTitle && m.ProductGender == genderDescription).FirstOrDefault()!;

                        if (sameNameProduct != null)
                        {
                            NewVariantRootModel newVariantRootModel = new();
                            NewVariantMerge newVariantMerge = new();
                            OverrideVariantImageUpdateModel overrideVariantImageUpdateModel = new();
                            NewVariantImageResponseModel newImage = new();
                            ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();
                            long[] variantIds;
                            NewVariantRequest newVariantRequest = new();

                            try
                            {
                                newVariantRequest.title = variant.title;
                                newVariantRequest.sku = variant.sku;
                                newVariantRequest.barcode = variant.sku;
                                newVariantRequest.price = variant.price;
                                newVariantRequest.weight = variant.weight;
                                newVariantRequest.inventory_management = variant.inventory_management;
                                newVariantRequest.inventory_policy = variant.inventory_policy;
                                newVariantRequest.fulfillment_service = variant.fulfillment_service;
                                newVariantRequest.option1 = variant.option1;
                                newVariantRequest.option2 = variant.option2;

                                newVariantMerge.variant = newVariantRequest;

                                newVariantRootModel = CreateNewVariant(newVariantMerge, sameNameProduct.ShopifyId!.ToString());

                                variantIds = new long[] { Convert.ToInt64(newVariantRootModel.variant.id) };

                                overrideVariantImageUpdateModel.image.product_id = Convert.ToInt64(sameNameProduct.ShopifyId);
                                overrideVariantImageUpdateModel.image.src = imageURL;
                                overrideVariantImageUpdateModel.image.variant_ids = variantIds;

                                newImage = CreateProductVariantImage(overrideVariantImageUpdateModel, overrideVariantImageUpdateModel.image.product_id);

                                shopifyInventoryDatum.ProductName = mainTitle.Split(",")[0];
                                shopifyInventoryDatum.ProductGender = genderDescription;
                                shopifyInventoryDatum.ShopifyId = sameNameProduct.ShopifyId.ToString();
                                shopifyInventoryDatum.BrandName = sameNameProduct.BrandName;
                                shopifyInventoryDatum.Sku = sku.Trim();
                                shopifyInventoryDatum.SkuPrefix = skuPrefix;
                                shopifyInventoryDatum.VariantId = newVariantRootModel.variant.id.ToString();
                                shopifyInventoryDatum.InventoryItemId = newVariantRootModel.variant.inventory_item_id.ToString();
                                shopifyInventoryDatum.ImageId = newImage.image.id.ToString();

                                shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDatum);

                                shopifyDBContextUpdateProduct.SaveChanges();

                                RefreshShopifySkusList();

                                UpdateProductStockQuantity(sku, 50);
                            }
                            catch (Exception ex)
                            {
                                LogErrorToFile(ex);
                            }
                        }
                        else
                        {
                            shopifyProductModelData.product.variants.Add(variant);
                            shopifyProductModelData.product.images.Add(image);

                            isVariantAdded = true;
                        }

                        AddMessageToLogs(fullSku + " : SKU merged");
                    }
                    else if(currentProduct!.SkuPrefix == fraganceXSkuPrefix)
                    {
                        UpdateProductStockQuantity(sku, productData.QuantityAvailable);
                        UpdateProductNewPrice(sku, currentProduct.VariantId!.ToString(), Convert.ToDecimal(updatedCost));
                    }
                }

                if (isVariantAdded)
                {
                    shopifyProductModelData.product.options.Add(WeightOption);
                    shopifyProductModelData.product.options.Add(GenderOption);

                    shopifyProductResponseData = CreateNewShopifyItem(shopifyProductModelData);

                    if (shopifyProductResponseData.product.variants.Count > 0)
                    {
                        List<ProductImageAttachVarient> productImageAttachVarientsList = new();

                        productImageAttachVarientsList = UpdateFragranceXProductVarientImages(shopifyProductResponseData, productsToProcessData);

                        AddMetaField(sku, shopifyProductResponseData.product.id.ToString());

                        foreach (Variant productVarient in shopifyProductResponseData.product.variants)
                        {
                            ShopifyInventoryDatum shopifyInventoryDatum = new ShopifyInventoryDatum();

                            shopifyInventoryDatum.ProductName = mainTitle;
                            shopifyInventoryDatum.ProductGender = genderDescription;
                            shopifyInventoryDatum.ShopifyId = shopifyProductResponseData.product.id.ToString();
                            shopifyInventoryDatum.BrandName = shopifyProductResponseData.product.vendor;
                            shopifyInventoryDatum.Sku = productVarient.sku.Substring(fraganceXSkuPrefix.Length, productVarient.sku.Length - fraganceXSkuPrefix.Length);
                            shopifyInventoryDatum.SkuPrefix = skuPrefix;
                            shopifyInventoryDatum.VariantId = productVarient.id.ToString();
                            shopifyInventoryDatum.InventoryItemId = productVarient.inventory_item_id.ToString();

                            try
                            {
                                shopifyInventoryDatum.ImageId = productImageAttachVarientsList.Where(m => m.image.variant_ids.Contains(productVarient.id)).FirstOrDefault()!.image.id.ToString();
                            }
                            catch (Exception ex)
                            {
                                LogErrorToFile(ex);
                            }
                            

                            shopifyInventoryDataList.Add(shopifyInventoryDatum);
                        }

                        if (shopifyInventoryDataList.Count > 0)
                        {
                            shopifyDBContextUpdateProduct.ShopifyInventoryData.AddRange(shopifyInventoryDataList);

                            shopifyDBContextUpdateProduct.SaveChanges();

                            AddMessageToLogs(Convert.ToString("--------New product created successfully--------"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }
        }

        private List<ProductImageAttachVarient> UpdateProductVarientImages(ShopifyProductModel shopifyProductModelData, CSVProductsToProcessModel csvProductsToProcessModel)
        {
            ShopifyProductModel shopifyProductData = new();
            List<ProductImageAttachVarient> productImageAttachVarientsList = new();

            try
            {
                shopifyProductData = shopifyProductModelData;

                foreach (Variant productVariant in shopifyProductData.product.variants)
                {
                    ProductImageAttachVarient productImageAttachVarient = new();
                    Image1 image1 = new Image1();
                    string imageURL = csvProductsToProcessModel.Products.Where(m => m.UPC == productVariant.sku.Substring(csvSkuPrefix.Length,productVariant.sku.Length - csvSkuPrefix.Length)).First().ImageURL;
                    string[] imageURLParts = imageURL.Split('/');
                    string imageName = imageURLParts[imageURLParts.Length - 1];
                    long[] variantIds = new long[] { productVariant.id };

                    imageName = imageName.Substring(0, imageName.Length - 4);

                    productImageAttachVarient.image.id = shopifyProductData.product.images.Where(m => m.src.Contains(imageName)).First().id;
                    productImageAttachVarient.image.variant_ids = variantIds;

                    UpdateProductVariantImage(productImageAttachVarient, productVariant.product_id);

                    productImageAttachVarientsList.Add(productImageAttachVarient);
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return productImageAttachVarientsList;
        }

        private List<ProductImageAttachVarient> UpdateFragranceXProductVarientImages(ShopifyProductModel shopifyProductModelData, FragranceXModel csvProductsToProcessModel)
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
                        string imageURL = csvProductsToProcessModel.products.Where(m => m.Upc == productVariant.sku.Substring(fraganceXSkuPrefix.Length, productVariant.sku.Length - fraganceXSkuPrefix.Length)).FirstOrDefault().LargeImageUrl;
                        string[] imageURLParts = imageURL.Split('/');
                        string imageName = imageURLParts[imageURLParts.Length - 1];
                        long[] variantIds = new long[] { productVariant.id };

                        imageName = imageName.Substring(0, imageName.Length - 4);

                        productImageAttachVarient.image.id = shopifyProductData.product.images.Where(m => m.src.Contains(imageName)).First().id;
                        productImageAttachVarient.image.variant_ids = variantIds;

                        UpdateProductVariantImage(productImageAttachVarient, productVariant.product_id);

                        productImageAttachVarientsList.Add(productImageAttachVarient);
                    }
                    catch (Exception ex)
                    {
                        LogErrorToFile(ex);
                    }                    
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return productImageAttachVarientsList;
        }

        private void UpdateProductStockQuantity(string sku,int quantity)
        {
            ShopifyProductInventoryModel shopifyProductInventoryData = new();
            ShopifyInventoryDatum ShopifyInventoryData = new();
            string shopifyID = string.Empty;
            string vendor = string.Empty;
            string inventoryItemId = string.Empty;
            List<string> restrictedSKus = new();

            try
            {
                ShopifyInventoryData  = shopifyProductsData.Where(m => m.Sku == sku).First<ShopifyInventoryDatum>();

                vendor = ShopifyInventoryData.BrandName!;

                if (restrictedBrandsList.Where(m => m.BrandName == vendor).ToList<RestrictedBrand>().Count > 0)
                {
                    AddMessageToLogs(Convert.ToString(vendor + " : Restricted Brand Found"));

                    restrictedSKus.Clear();

                    restrictedSKus.Add(sku);

                    UpdateSKUAsRestricted(restrictedSKus);

                    return;
                }

                if (restrictedSkusList.Where(m => m.Sku == sku).ToList<RestrictedSku>().Count > 0)
                {
                    AddMessageToLogs(Convert.ToString(sku + " : Restricted SKU Found"));

                    restrictedSKus.Clear();

                    restrictedSKus.Add(sku);

                    UpdateSKUAsRestricted(restrictedSKus);

                    return;
                }

                shopifyID = ShopifyInventoryData.ShopifyId!;
                inventoryItemId = ShopifyInventoryData.InventoryItemId!;

                if (GetApplicationSettings("MarkOutOfStock").ToUpper() == "Y")
                {
                    shopifyProductInventoryData.inventory_item_id = Convert.ToInt64(inventoryItemId);
                    shopifyProductInventoryData.location_id = Convert.ToInt64(defaultLocationID);
                    shopifyProductInventoryData.available = quantity;

                    SetProductInventoryLevel(shopifyProductInventoryData);

                    if(quantity <= 0)
                    {
                        AddMessageToLogs(Convert.ToString(sku + " : Marked out of stock"));

                        MarkProductOutOfStock(sku, true);
                    }
                    else
                    {
                        AddMessageToLogs(Convert.ToString(sku + " : Quantity Updated - " + quantity.ToString()));

                        MarkProductOutOfStock(sku, false);
                    }
                }
                else
                {
                    AddMessageToLogs(Convert.ToString(sku + " : Modification is not allowed"));
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }
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
                    UpdateVariantPrice(singleVariantPriceUpdate);
                }

                AddMessageToLogs(Convert.ToString(sku + " : Price Updated  to : " + updatedPrice.ToString())); 
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }
        }

        private bool UpdateVariantPrice(SingleVariantPriceUpdate singleVariantPriceUpdate)
        {
            string url = GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2022-10/variants/"+ singleVariantPriceUpdate.variant.id.ToString() +".json";
            bool result = false;

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(url,Method.Put);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", JsonConvert.SerializeObject(singleVariantPriceUpdate), ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return result;
        }

        private bool MarkProductOutOfStock(string sku,bool isOutOfStock)
        {
            ShopifyInventoryDatum ShopifyInventoryData = new();
            ShopifyDbContext shopifyDBContextUpdateProduct = new ShopifyDbContext();
            bool result = false;

            try
            {
                ShopifyInventoryData = shopifyDBContextUpdateProduct.ShopifyInventoryData.Where(m=>m.Sku == sku).First();

                ShopifyInventoryData.IsOutOfStock = isOutOfStock;

                shopifyDBContextUpdateProduct.ShopifyInventoryData.Update(ShopifyInventoryData);

                shopifyDBContextUpdateProduct.SaveChanges();

                result = true;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return result;
        }

        private void IncrementProgressBar()
        {
            if(progressBarValue < 90)
            {
                progressBarValue += progressBarIncrementValue; 
            }
            else
            {
                progressBarValue = 100;
            }

            lblProgressCount.Text = ((int)progressBarValue).ToString() + "% Completed";

            ProcessProgress.Value = (int)progressBarValue;
        }

        private ShopifyProductModel CreateNewShopifyItem(ShopifyProductModel shopifyProductModelData)
        {

            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products.json", Method.Post);
                
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                var body = JsonConvert.SerializeObject(shopifyProductModelData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                shopifyProductResponseData = JsonConvert.DeserializeObject<ShopifyProductModel>(response.Content!)!;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        private Image1 UpdateProductVariantImage(ProductImageAttachVarient shopifyProductImageData, long productId)
        {

            Image1 shopifyProductResponseData = new Image1();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products/" + productId.ToString() + "/images/"+ shopifyProductImageData.image.id + ".json", Method.Put);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                var body = JsonConvert.SerializeObject(shopifyProductImageData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);

                shopifyProductResponseData = JsonConvert.DeserializeObject<Image1>(response.Content!)!;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        private NewVariantImageResponseModel CreateProductVariantImage(OverrideVariantImageUpdateModel overrideVariantImageUpdateModel, long productId)
        {
            NewVariantImageResponseModel image = new NewVariantImageResponseModel();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products/" + productId.ToString() + "/images.json", Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
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
                LogErrorToFile(ex);
            }

            return image;
        }

        private bool DeleteProductVariantImage(long productId, long imageID)
        {
            bool result = false;

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products/" + productId.ToString() + "/images/"+ imageID.ToString() + ".json", Method.Delete);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return result;
        }

        private ShopifyProductModel FetchProductFromShopify(string shopifyID)
        {
            ShopifyProductModel shopifyProductResponseData = new ShopifyProductModel();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2021-10/products/" + shopifyID + ".json",Method.Get);
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                RestResponse response = client.Execute(request);

                shopifyProductResponseData = JsonConvert.DeserializeObject<ShopifyProductModel>(response.Content!)!;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        private ShopifyProductInventoryResponse SetProductInventoryLevel(ShopifyProductInventoryModel shopifyProductModelData)
        {
            ShopifyProductInventoryResponse shopifyProductResponseData = new();

            try
            {
                RestClient client = new RestClient();
                RestRequest request = new RestRequest(GetApplicationSettings("ShopifyBaseURL") + "/admin/api/2022-01/inventory_levels/set.json", Method.Post);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
                var body = JsonConvert.SerializeObject(shopifyProductModelData);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                RestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);

                shopifyProductResponseData = JsonConvert.DeserializeObject<ShopifyProductInventoryResponse>(response.Content!)!;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return shopifyProductResponseData;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedAPI == (int)SharedData.APIType.CSV)
            {
                List<CSVProductsToProcessModel> productsDataListPrepared = new List<CSVProductsToProcessModel>();

                if (productsDataTable != null)
                {
                    if (productsDataTable.Rows.Count > 0)
                    {
                        productsDataListPrepared = PrepareProductsDataToPost(productsDataList);

                        ProcessCSVData(productsDataListPrepared);
                    }
                    else
                    {
                        MessageBox.Show("Please load load CSV file to process");
                    }
                }
                else
                {
                    MessageBox.Show("Please load CSV file to process");
                }
            }
            else if (selectedAPI == (int)SharedData.APIType.FragranceX)
            {
                List<FragranceXModel> productsDataListPrepared = new List<FragranceXModel>();

                if (productsDataTable != null)
                {
                    if (productsDataTable.Rows.Count > 0)
                    {
                        productsDataListPrepared = PrepareFragranceXProductsDataToPost(fragranceXProducts);

                        ProcessFragranceXData(productsDataListPrepared);
                    }
                    else
                    {
                        MessageBox.Show("No record found to process");
                    }
                }
                else
                {
                    MessageBox.Show("No record found to process");
                }
            }
        }

        private List<CSVProductsToProcessModel> PrepareProductsDataToPost(List<ProductsCSVModel> productsList)
        {
            List<ProductsCSVModel> productsListPrePrepare = new List<ProductsCSVModel>();
            List<ProductsCSVModel> productsListPostPrepare = new List<ProductsCSVModel>();
            List<CSVProductsToProcessModel> productsDataListPrepared = new List<CSVProductsToProcessModel>();

            ProductsCSVModel selectedProduct = new();
            string productName = string.Empty;
            string genderMale = string.Empty;
            string genderFemale = string.Empty;

            try
            {
                productsDataListPrepared.Clear();

                productsListPrePrepare = productsList;

                while (productsListPrePrepare.Count > 0)
                {
                    CSVProductsToProcessModel csvProductsToProcessModel = new();

                    selectedProduct = productsListPrePrepare.First();

                    productName = selectedProduct.Name.Split(',')[0].ToString();
                    genderMale = selectedProduct.Men;
                    genderFemale = selectedProduct.Women;

                    productsListPostPrepare = productsListPrePrepare.Where(m => m.Name.Split(',')[0].ToString() == productName && m.Men == genderMale && m.Women == genderFemale).ToList<ProductsCSVModel>();

                    csvProductsToProcessModel.Products = productsListPostPrepare;

                    productsDataListPrepared.Add(csvProductsToProcessModel);

                    productsListPrePrepare.RemoveAll(m => m.Name.Split(',')[0].ToString() == productName && m.Men == genderMale && m.Women == genderFemale);
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

            return productsDataListPrepared;
        }

        private List<FragranceXModel> PrepareFragranceXProductsDataToPost(List<FragranceXProduct> productsList)
        {
            List<FragranceXProduct> productsListPrePrepare = new List<FragranceXProduct>();
            List<FragranceXProduct> productsListPostPrepare = new List<FragranceXProduct>();
            List<FragranceXModel> productsDataListPrepared = new List<FragranceXModel>();

            FragranceXProduct selectedProduct = new();
            string productName = string.Empty;
            string gender = string.Empty;

            try
            {
                productsDataListPrepared.Clear();

                productsListPrePrepare = productsList;

                while (productsListPrePrepare.Count > 0)
                {
                    FragranceXModel csvProductsToProcessModel = new();

                    selectedProduct = productsListPrePrepare.First();

                    productName = selectedProduct.ProductName;
                    gender = selectedProduct.Gender;

                    productsListPostPrepare = productsListPrePrepare.Where(m => m.ProductName == productName && m.Gender == gender).ToList<FragranceXProduct>();

                    csvProductsToProcessModel.products = productsListPostPrepare;

                    productsDataListPrepared.Add(csvProductsToProcessModel);

                    productsListPrePrepare.RemoveAll(m => m.ProductName == productName && m.Gender == gender);
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

            return productsDataListPrepared;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileInfo fileInfo;
            OpenFileDialog openFileDialog = new OpenFileDialog();

            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    ClearGridData();

                    selectedCSVFile = openFileDialog.FileName;

                    fileInfo = new FileInfo(selectedCSVFile);

                    if (fileInfo.Extension.ToUpper() == ".CSV")
                    {
                        try
                        {
                            using (var reader = new StreamReader(selectedCSVFile))
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                productsDataList.Clear();

                                productsDataList = csv.GetRecords<ProductsCSVModel>().ToList<ProductsCSVModel>();

                                productsDataTable = SharedFunctions.LinqToDataTable<ProductsCSVModel>(productsDataList);

                                shopifyProductsToRemove = (from s in shopifyProductsData
                                            where !productsDataList.Any(x=>x.UPC == s.Sku && s.SkuPrefix == csvSkuPrefix)
                                            select s).Where(s => s.SkuPrefix != fraganceXSkuPrefix).ToList<ShopifyInventoryDatum>();

                                loadedDataGridView.DataSource = productsDataTable;

                                loadedDataGridView.Columns["sku"].Visible = false;
                                loadedDataGridView.Columns["retail"].Visible = false;
                                loadedDataGridView.Columns["weight"].Visible = false;
                                loadedDataGridView.Columns["inventory"].Visible = false;

                                btnProcess.Enabled = true;

                                selectedAPI = (int)SharedData.APIType.CSV;

                                lblSelectedAPI.Text = "CSV API";
                            }
                        }
                        catch (Exception ex)
                        {
                            LogErrorToFile(ex);

                            MessageBox.Show(ex.Message);
                        }
                    }
                    else
                    {
                        MessageBox.Show("please provide only csv files");
                    }
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form applicationSettingsForm;

            try
            {
                applicationSettingsForm = new ApplicationSettingsForm();

                applicationSettingsForm.ShowDialog();

                RefreshApplicationSettingsList();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void markupSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form markUpPricesForm;

            try
            {
                markUpPricesForm = new MarkUpPricesForm();

                markUpPricesForm.ShowDialog();

                RefreshMarkUPPricesList();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form aboutForm = new AboutForm();

            aboutForm.ShowDialog();
        }

        private string GetApplicationSettings(string tag)
        {
            string tagValue = string.Empty;

            try
            {
                tagValue = applicationSettingsList.Where(m => m.Tag == tag).First<ApplicationSetting>().TagValue!;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

            return tagValue;
        }

        private void LogErrorToFile(Exception ex)
        {
            SharedFunctions.WriteToErrorLog(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace);
        }

        private decimal CalculateMarkupPrice(decimal actualPrice)
        {
            decimal markedupPrice = actualPrice;

            foreach (MarkUpPrice markUpPriceItem in markUpPricesList)
            {
                if(markUpPriceItem.MinPrice <= Math.Ceiling(actualPrice) && markUpPriceItem.MaxPrice >= Math.Ceiling(actualPrice))
                {
                    markedupPrice = Math.Round(actualPrice + ((markUpPriceItem.MarkupPercentage * actualPrice) / 100),2);

                    break;
                }
            }

            return markedupPrice;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearGridData();

                txtProcessedProducts.Text = String.Empty;

                btnProcess.Enabled = false;
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void restrictedBrandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestrictedBrandsForm restrictedBrandsForm;

            try
            {
                restrictedBrandsForm = new();

                restrictedBrandsForm.ShowDialog();

                RefreshRestrictedBrandsList();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void restrictedSKUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestrictedSkusForm restrictedSkusForm;

            try
            {
                restrictedSkusForm = new();

                restrictedSkusForm.ShowDialog();

                RefreshRestrictedSkusList();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void AddMessageToLogs(string message)
        {
            processingMessages += message + Environment.NewLine;
        }

        private void fetchFragranceXProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string fragranceXJsonData = string.Empty;
            List<FragranceXProduct> loadedFragranceXProducts = new List<FragranceXProduct>();
            string token = string.Empty;

            try
            {
                ClearGridData();

                selectedAPI = (int)SharedData.APIType.FragranceX;
                lblSelectedAPI.Text = "FragranceX API";

                token = GetFragranceXAPIToken();

                if (!string.IsNullOrEmpty(token))
                {
                    fragranceXJsonData = GetFragranceXAPIData(token);

                    //fragranceXJsonData = File.ReadAllText(Environment.CurrentDirectory + "//FragranceX_Single.txt");

                    fragranceXProducts.Clear();

                    loadedFragranceXProducts = JsonConvert.DeserializeObject<List<FragranceXProduct>>(fragranceXJsonData!)!;

                    if(loadedFragranceXProducts != null)
                    {
                        if (loadedFragranceXProducts.Count <= 0)
                        {
                            btnProcess.Enabled = false;

                            MessageBox.Show("No data found to process");

                            return;
                        }

                        fragranceXProducts = loadedFragranceXProducts.Where(m => m.Upc != "").ToList();

                        productsDataTable = SharedFunctions.LinqToDataTable<FragranceXProduct>(fragranceXProducts);

                        shopifyProductsToRemove = (from s in shopifyProductsData
                                                   where !fragranceXProducts.Any(x => x.Upc == s.Sku && s.SkuPrefix == fraganceXSkuPrefix)
                                                   select s).Where(s => s.SkuPrefix != csvSkuPrefix).ToList<ShopifyInventoryDatum>();

                        loadedDataGridView.DataSource = productsDataTable;

                        if (fragranceXProducts.Count > 0)
                        {
                            btnProcess.Enabled = true;
                        }
                        else
                        {
                            btnProcess.Enabled = false;

                            MessageBox.Show("No data found to process");
                        }
                    }                    
                }
                else
                {
                    btnProcess.Enabled = false;

                    MessageBox.Show("FragranceX API not accessible");
                }

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private String GetFragranceXAPIToken()
        {
            string token = string.Empty;
            string url = GetApplicationSettings("FragrancexURL") + "/token";
            FragranceXToken fragranceXToken = new();

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url,Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("grant_type", GetApplicationSettings("grant_type"));
                request.AddParameter("apiAccessId", GetApplicationSettings("apiAccessId"));
                request.AddParameter("apiAccessKey", GetApplicationSettings("apiAccessKey"));

                RestResponse response = client.Execute(request);

                if(response.StatusCode == HttpStatusCode.OK)
                {
                    fragranceXToken = JsonConvert.DeserializeObject<FragranceXToken>(response.Content!)!;

                    token = fragranceXToken.access_token;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show("Unable to connect FragranceX API");
            }

            return token;
        }

        private String GetFragranceXAPIData(string token)
        {
            string productsData = string.Empty;
            string url = GetApplicationSettings("FragrancexURL") + "/product/list/";
            FragranceXToken fragranceXToken = new();

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url, Method.Get);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);

                RestResponse response = client.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    productsData = response.Content!;
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return productsData;
        }

        private void fixedPricesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixedPricesForm fixedPricesForm;

            try
            {
                fixedPricesForm = new();

                fixedPricesForm.ShowDialog();

                RefreshFixedPricesList();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private bool AddMetaField(string sku,string shopifyID)
        {
            string url = "https://scent-bin.myshopify.com/admin/api/2022-10/products/"+ shopifyID + "/metafields.json";
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
                request.AddHeader("X-Shopify-Access-Token", GetApplicationSettings("ShopifyAccessKey"));
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
                LogErrorToFile(ex);
            }

            return result;
        }
    }
}