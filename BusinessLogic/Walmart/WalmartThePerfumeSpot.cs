using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic.Vendors;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Linq;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    internal class WalmartThePerfumeSpot
    {
        ApplicationState applicationState;

        private readonly IWalmartFeedResponseRepository walmartFeedResponseRepository;
        private readonly IWalmartInventoryDataRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly WalmartAPI walmartAPI;
        private readonly ThePerfumeSpotAPI thePerfumeSpotAPI;

        public WalmartThePerfumeSpot()
        {
            applicationState = ApplicationState.GetState;
            walmartAPI = new();
            thePerfumeSpotAPI = new();
            productsRepository = new WalmartInventoryDataRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();
            walmartFeedResponseRepository = new WalmartFeedResponseRepository();
        }

        public List<ThePerfumeSpotProduct> FilterOutOfStockProducts(ThePerfumeSpotProductsList productsList)
        {
            List<WalmartInventoryDatum> productsOutOfStock = new();
            List<ThePerfumeSpotProduct> productsOutOfStockList = new();
            List<ThePerfumeSpotProduct> products = new();

            try
            {
                products = productsList.products;

                productsOutOfStock = (from s in productsRepository.GetBySkuPrefix(TPSSKUPREFIX)
                                    where !products.Any(x => x.UPC == s.Sku)
                                    select s).ToList();

                foreach (WalmartInventoryDatum item in productsOutOfStock)
                {
                    ThePerfumeSpotProduct thePerfumeSpotProduct = new();

                    thePerfumeSpotProduct.UPC = item.Sku!;

                    productsOutOfStockList.Add(thePerfumeSpotProduct);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsOutOfStockList;
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
                                    where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == TPSSKUPREFIX))
                                    select s).ToList<RestrictedBrand>();

                restrictedSku = (from s in restrictedSkusRepository.GetAll()
                                 where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == TPSSKUPREFIX))
                                 select s).ToList<RestrictedSku>();

                productsToRemove = (from s in productsList.products
                                    where restrictedSku.Any(x => x.Sku == s.UPC) || restrictedBrands.Any(x => x.BrandName!.Trim().ToUpper() == s.Brand.Trim().ToUpper())
                                    select s).ToList<ThePerfumeSpotProduct>();

                productsRemoveSaveData = (from s in productsRepository.GetAll()
                                          where productsToRemove.Any(m => m.UPC == s.Sku && s.SkuPrefix == TPSSKUPREFIX)
                                          select s).ToList<WalmartInventoryDatum>();

                walmartInventoryRepository.DeleteMultiple(productsRemoveSaveData);

                walmartInventoryRepository.Save();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<ThePerfumeSpotProduct> FilterProductsToProcess(ThePerfumeSpotProductsList productsList, List<ThePerfumeSpotProduct> removedProducts)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<ThePerfumeSpotProduct> productsToProcess = new();
            List<ThePerfumeSpotProduct> productsToIgnore = new();
            List<ThePerfumeSpotProduct> productsToSave = new();
            List<WalmartInventoryDatum> productsToUpdate = new();
            List<WalmartInventoryDatum> productsAddedSaveData = new();
            List<WalmartInventoryDatum> allWalmartProducts = new();

            try
            {
                allWalmartProducts = productsRepository.GetAll().ToList<WalmartInventoryDatum>();

                productsToIgnore = (from s in productsList.products
                                    where (allWalmartProducts.Any(m => m.Sku == s.UPC && m.SkuPrefix != TPSSKUPREFIX && m.Price < Convert.ToDecimal(s.YourCost)))
                                    select s).ToList<ThePerfumeSpotProduct>();

                productsToProcess = (from s in productsList.products
                                    where !(removedProducts.Any(x => x.UPC == s.UPC) || productsToIgnore.Any(x => x.UPC == s.UPC))
                                    select s).ToList<ThePerfumeSpotProduct>();

                productsToSave = (from s in productsToProcess
                                where !allWalmartProducts.Any(m => m.Sku == s.UPC)
                                select s).ToList<ThePerfumeSpotProduct>();

                productsToUpdate = (from s in walmartInventoryRepository.GetAll()
                                    where productsToProcess.Any(x => x.UPC == s.Sku)
                                    select s).ToList<WalmartInventoryDatum>();

                foreach (ThePerfumeSpotProduct product in productsToSave)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = TPSSKUPREFIX;
                    walmartInventoryDatum.Sku = product.UPC;
                    walmartInventoryDatum.BrandName = product.Brand.Trim();
                    walmartInventoryDatum.IsShippingMapped = false;
                    walmartInventoryDatum.Price = Convert.ToDecimal(product.YourCost);

                    productsAddedSaveData.Add(walmartInventoryDatum);
                }

                productsToUpdate.ForEach(x => 
                {
                    x.Price = Convert.ToDecimal(productsToProcess.Where(m => m.UPC == x.Sku).First().YourCost);
                    x.SkuPrefix = TPSSKUPREFIX;
                });

                walmartInventoryRepository.InsertMultiple(productsAddedSaveData);
                walmartInventoryRepository.UpdateMultiple(productsToUpdate);

                walmartInventoryRepository.Save();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToProcess;
        }

        public List<string> FormatSourceProductsData(List<ThePerfumeSpotProduct> productsList)
        {
            string productName = string.Empty;
            string genderMale = string.Empty;
            string genderFemale = string.Empty;
            WalmartProductModel walmartProductModel = new();
            List<string> productsData = new List<string>();
            IEnumerable<ThePerfumeSpotProduct[]> thePerfumeSpotProductsMultiLists;

            try
            {
                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

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
                        decimal calculatedCost = 0;

                        sku = productData.UPC;
                        fullSku = TPSSKUPREFIX + productData.UPC;
                        vendor = productData.Brand;
                        mainTitle = productData.Name.Split(',')[0].ToString();
                        calculatedCost = applicationState.GetMarkedUpPrice(sku, productData.YourCost, STORENAME.WALMART);

                        mpitem.Orderable.sku = fullSku;
                        mpitem.Orderable.productIdentifiers.productIdType = "GTIN";
                        mpitem.Orderable.productIdentifiers.productId = sku.PadLeft(14, '0');                        

                        mpitem.Orderable.productName = mainTitle;
                        mpitem.Orderable.brand = vendor;
                        mpitem.Orderable.price = calculatedCost < Convert.ToDecimal(WALMARTMINPRICELEVEL) ? Convert.ToDecimal(WALMARTMINPRICELEVEL) : calculatedCost;
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

        public List<string> FormatSourceProductsInventoryData(List<ThePerfumeSpotProduct> productsList, List<ThePerfumeSpotProduct> outOfStockList, bool markOutOfStock)
        {
            WalmartInventoryRequestModel walmartInventoryRequestModel = new();
            IEnumerable<ThePerfumeSpotProduct[]> thePerfumeSpotProductsMultiLists;
            List<string> productsData = new List<string>();

            try
            {
                productsList.AddRange(outOfStockList);

                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartInventoryRequestModel.inventoryHeader.version = "1.5";

                foreach (ThePerfumeSpotProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartInventoryRequestModel.inventory.Clear();

                    foreach (ThePerfumeSpotProduct productData in productsSingleList)
                    {
                        Inventory inventory = new();
                        Shipnode shipnode = new();

                        inventory.sku = TPSSKUPREFIX + productData.UPC;

                        shipnode.shipNode = FULFILLMENTCENTERID;
                        shipnode.quantity.unit = "EACH";
                        shipnode.quantity.amount = outOfStockList.Where(m => m.UPC == productData.UPC).ToList().Count > 0 ? 0 : Convert.ToInt32(MINIMUMQUANTITY);

                        inventory.shipNodes.Add(shipnode);

                        walmartInventoryRequestModel.inventory.Add(inventory);
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

        public List<string> FormatShippingTemplateMappingData(List<ThePerfumeSpotProduct> productsList)
        {
            WalmartShippingTemplateMapping walmartShippingTemplate = new();
            IEnumerable<ThePerfumeSpotProduct[]> thePerfumeSpotProductsMultiLists;
            List<ThePerfumeSpotProduct> productsListToMap;
            List<string> productsData = new List<string>();
            List<WalmartInventoryDatum> walmartInventoryDatumList = new();
            WalmartInventoryDataRepository walmartInventoryRepository = new();

            try
            {
                productsListToMap = (from s in productsList
                                    where (productsRepository.GetBySkuPrefix(TPSSKUPREFIX).Any(m => m.Sku == s.UPC && m.IsShippingMapped == false))
                                    select s).ToList<ThePerfumeSpotProduct>();

                thePerfumeSpotProductsMultiLists = productsListToMap.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartShippingTemplate.ItemFeedHeader.sellingChannel = "precisedelivery";
                walmartShippingTemplate.ItemFeedHeader.locale = "en";
                walmartShippingTemplate.ItemFeedHeader.version = "1.0";

                foreach (ThePerfumeSpotProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartShippingTemplate.Item.Clear();

                    foreach (ThePerfumeSpotProduct productData in productsSingleList)
                    {
                        ShippingTemplateItem shippingTemplateItem = new();
                        Precisedelivery precisedelivery = new();

                        precisedelivery.sku = TPSSKUPREFIX + productData.UPC;
                        precisedelivery.actionType = "Add";
                        precisedelivery.shippingTemplateId = SHIPPINGTEMPLATEID;
                        precisedelivery.fulfillmentCenterId = FULFILLMENTCENTERID;

                        shippingTemplateItem.PreciseDelivery = precisedelivery;

                        walmartShippingTemplate.Item.Add(shippingTemplateItem);
                    }

                    productsData.Add(JsonConvert.SerializeObject(walmartShippingTemplate));
                }

                walmartInventoryDatumList = (from s in productsRepository.GetBySkuPrefix(TPSSKUPREFIX)
                                             where productsListToMap.Any(m => m.UPC == s.Sku)
                                             select s).ToList<WalmartInventoryDatum>();

                walmartInventoryDatumList.ForEach(c => c.IsShippingMapped = true);

                walmartInventoryRepository.UpdateMultiple(walmartInventoryDatumList);

                walmartInventoryRepository.Save();
            }
            catch (Exception)
            {
                throw;
            }

            return productsData;
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
