using CsvHelper;
using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic.Shopify;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Globalization;
using System.Text;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    internal class WalmartThePerfumeSpot
    {
        ApplicationState applicationState;

        private readonly IWalmartFeedResponseRepository walmartFeedResponseRepository;
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
            walmartFeedResponseRepository = new WalmartFeedResponseRepository();
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
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<ThePerfumeSpotProduct> productsToRemove = new();
            List<RestrictedBrand> restrictedBrands = new List<RestrictedBrand>();
            List<RestrictedSku> restrictedSku = new List<RestrictedSku>();
            List<WalmartInventoryDatum> productsRemoveSaveData = new();

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

                foreach (ThePerfumeSpotProduct product in productsToRemove)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = GlobalConstants.tpsSKUPrefix;
                    walmartInventoryDatum.Sku = product.UPC;
                    walmartInventoryDatum.BrandName = product.Brand;

                    productsRemoveSaveData.Add(walmartInventoryDatum);
                }

                walmartInventoryRepository.DeleteMultiple(productsRemoveSaveData);

                walmartInventoryRepository.Save();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<ThePerfumeSpotProduct> FilterProductsToProcess(ThePerfumeSpotProductsList productsList, List<ThePerfumeSpotProduct> removedProducts, List<WalmartInventoryDatum> outOfStockProducts)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<ThePerfumeSpotProduct> productsToProcess = new(); ;
            List<ThePerfumeSpotProduct> productsToSave = new();
            List<WalmartInventoryDatum> productsAddedSaveData = new();

            try
            {
                productsToProcess = (from s in productsList.products
                                    where (!removedProducts.Any(x => x.UPC == s.UPC) || !outOfStockProducts.Any(x => x.Sku == s.UPC))
                                    select s).ToList<ThePerfumeSpotProduct>();

                productsToSave = (from s in productsToProcess
                                where !productsRepository.GetAll().Any(m => m.Sku == s.UPC)
                                select s).ToList<ThePerfumeSpotProduct>();

                foreach (ThePerfumeSpotProduct product in productsToSave)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = GlobalConstants.tpsSKUPrefix;
                    walmartInventoryDatum.Sku = product.UPC;
                    walmartInventoryDatum.BrandName = product.Brand;

                    productsAddedSaveData.Add(walmartInventoryDatum);
                }

                walmartInventoryRepository.InsertMultiple(productsAddedSaveData);

                walmartInventoryRepository.Save();
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
                        string vendor = string.Empty;
                        string mainTitle = string.Empty;

                        sku = productData.UPC;
                        fullSku = GlobalConstants.tpsSKUPrefix + productData.UPC;
                        vendor = productData.Brand;
                        mainTitle = productData.Name.Split(',')[0].ToString();

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

                walmartInventoryRequestModel.InventoryHeader.version = "1.5";

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

        public void ProcessProductToWalmart(string walmartProductsTextData, GlobalConstants.WALMARTFEEDTYPE wALMARTFEEDTYPE)
        {
            try
            {
                string response = walmartAPI.PostProductsToWalmart(walmartProductsTextData, wALMARTFEEDTYPE);

                SaveWalmartFeed(response);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private void SaveWalmartFeed(string response)
        {
            WalmartFeedObject walmartFeed = new();
            WalmartFeedResponse walmartFeedResponse = new();

            try
            {
                walmartFeed = JsonConvert.DeserializeObject<WalmartFeedObject>(response)!;

                walmartFeedResponse.FeedId = walmartFeed.feedId;
                walmartFeedResponse.EcomStoreId = (int)GlobalConstants.STORENAME.WALMART;

                walmartFeedResponseRepository.Insert(walmartFeedResponse);

                walmartFeedResponseRepository.Save();
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
