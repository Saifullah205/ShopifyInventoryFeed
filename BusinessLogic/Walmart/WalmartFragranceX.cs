﻿using CsvHelper;
using Newtonsoft.Json;
using RestSharp;
using ShopifyInventorySync.BusinessLogic.Shopify;
using ShopifyInventorySync.BusinessLogic.Vendors;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Globalization;
using System.Net;
using System.Text;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    internal class WalmartFragranceX
    {
        ApplicationState applicationState;

        private readonly IWalmartFeedResponseRepository walmartFeedResponseRepository;
        private readonly IWalmartInventoryDataRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly WalmartAPI walmartAPI;
        private readonly FragranceXAPI fragranceXAPI;

        public WalmartFragranceX()
        {
            applicationState = ApplicationState.GetState;
            walmartAPI = new();
            productsRepository = new WalmartInventoryDataRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();
            walmartFeedResponseRepository = new WalmartFeedResponseRepository();
            fragranceXAPI = new();
        }

        public FragranceXProductsList GetDataFromSource()
        {
            FragranceXProductsList fragranceXProductsList = new();

            try
            {
                fragranceXProductsList.products = fragranceXAPI.FetchDataFromAPI().Where(m => m.Upc != "").ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceXProductsList;
        }

        public List<WalmartInventoryDatum> FilterOutOfStockProducts(FragranceXProductsList productsList)
        {
            List<WalmartInventoryDatum> productsToRemove = new();
            List<FragranceXProduct> products = new();

            try
            {
                products = productsList.products;

                productsToRemove = (from s in productsRepository.GetBySkuPrefix(GlobalConstants.fragranceXSKUPrefix)
                                    where !products.Any(x => x.Upc == s.Sku)
                                    select s).ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<FragranceXProduct> FilterProductsToRemove(FragranceXProductsList productsList)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceXProduct> productsToRemove = new();
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
                                    where restrictedSku.Any(x => x.Sku == s.Upc) || restrictedBrands.Any(x => x.BrandName == s.BrandName)
                                    select s).ToList<FragranceXProduct>();

                foreach (FragranceXProduct product in productsToRemove)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = GlobalConstants.fragranceXSKUPrefix;
                    walmartInventoryDatum.Sku = product.Upc;
                    walmartInventoryDatum.BrandName = product.BrandName;

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

        public List<FragranceXProduct> FilterProductsToProcess(FragranceXProductsList productsList, List<FragranceXProduct> removedProducts, List<WalmartInventoryDatum> outOfStockProducts)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceXProduct> productsToProcess = new(); ;
            List<FragranceXProduct> productsToSave = new();
            List<WalmartInventoryDatum> productsAddedSaveData = new();

            try
            {
                productsToProcess = (from s in productsList.products
                                    where (!removedProducts.Any(x => x.Upc == s.Upc) || !outOfStockProducts.Any(x => x.Sku == s.Upc))
                                    select s).ToList<FragranceXProduct>();

                productsToSave = (from s in productsToProcess
                                where !productsRepository.GetAll().Any(m => m.Sku == s.Upc)
                                select s).ToList<FragranceXProduct>();

                foreach (FragranceXProduct product in productsToSave)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = GlobalConstants.fragranceXSKUPrefix;
                    walmartInventoryDatum.Sku = product.Upc;
                    walmartInventoryDatum.BrandName = product.BrandName;

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

        public List<WalmartInventoryDatum> PrepareInStockProductsQtyToProcess(List<FragranceXProduct> productsList)
        {
            List<WalmartInventoryDatum> productsToProcess = new();

            try
            {
                foreach (FragranceXProduct item in productsList)
                {
                    WalmartInventoryDatum thePerfumeSpotProduct = new();

                    thePerfumeSpotProduct.Sku = item.Upc;

                    productsToProcess.Add(thePerfumeSpotProduct);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsToProcess;
        }

        public List<string> FormatSourceProductsData(List<FragranceXProduct> productsList)
        {
            string productName = string.Empty;
            string genderMale = string.Empty;
            string genderFemale = string.Empty;
            WalmartProductModel walmartProductModel = new();
            List<string> productsData = new List<string>();
            IEnumerable<FragranceXProduct[]> thePerfumeSpotProductsMultiLists;

            try
            {
                thePerfumeSpotProductsMultiLists = productsList.Chunk(8000);

                walmartProductModel.MPItemFeedHeader.sellingChannel = "marketplace";
                walmartProductModel.MPItemFeedHeader.processMode = "REPLACE";
                walmartProductModel.MPItemFeedHeader.subset = "EXTERNAL";
                walmartProductModel.MPItemFeedHeader.locale = "en";
                walmartProductModel.MPItemFeedHeader.version = "1.5";
                walmartProductModel.MPItemFeedHeader.subCategory = "office_other";

                foreach (FragranceXProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartProductModel.MPItem.Clear();

                    foreach (FragranceXProduct productData in productsSingleList)
                    {
                        Mpitem mpitem = new();
                        string sku = string.Empty;
                        string fullSku = string.Empty;
                        string vendor = string.Empty;
                        string mainTitle = string.Empty;

                        sku = productData.Upc;
                        fullSku = GlobalConstants.fragranceXSKUPrefix + productData.Upc;
                        vendor = productData.BrandName;
                        mainTitle = productData.ProductName + " by " + vendor;

                        mpitem.Orderable.sku = fullSku;
                        mpitem.Orderable.productIdentifiers.productIdType = "GTIN";
                        mpitem.Orderable.productIdentifiers.productId = sku.PadLeft(14, '0');

                        mpitem.Orderable.productName = mainTitle;
                        mpitem.Orderable.brand = vendor;
                        mpitem.Orderable.price = applicationState.GetMarkedUpPrice(sku, productData.WholesalePriceUSD.ToString(), GlobalConstants.STORENAME.WALMART);
                        mpitem.Orderable.ShippingWeight = 0;
                        mpitem.Orderable.electronicsIndicator = "No";
                        mpitem.Orderable.batteryTechnologyType = "Does Not Contain a Battery";
                        mpitem.Orderable.chemicalAerosolPesticide = "No";
                        mpitem.Orderable.shipsInOriginalPackaging = "No";
                        mpitem.Orderable.startDate = "2019-01-01T08:00:00Z";
                        mpitem.Orderable.endDate = "2060-01-01T08:00:00Z";
                        mpitem.Orderable.MustShipAlone = "No";

                        mpitem.Visible.Office.shortDescription = "Storage";
                        mpitem.Visible.Office.mainImageUrl = productData.LargeImageUrl;
                        mpitem.Visible.Office.productSecondaryImageURL = new List<string> { productData.LargeImageUrl };
                        mpitem.Visible.Office.prop65WarningText = "None";
                        mpitem.Visible.Office.smallPartsWarnings = new List<string> { "0 - No warning applicable" };
                        mpitem.Visible.Office.compositeWoodCertificationCode = "1 - Does not contain composite wood";
                        mpitem.Visible.Office.keyFeatures = new List<string> { mainTitle };
                        mpitem.Visible.Office.manufacturer = productData.BrandName;
                        mpitem.Visible.Office.manufacturerPartNumber = productData.Upc;

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

                        inventory.sku = GlobalConstants.fragranceXSKUPrefix + productData.Sku;
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
    }
}