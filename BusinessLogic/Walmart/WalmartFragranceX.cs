using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic.Vendors;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

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

        public List<FragranceXProduct> FilterOutOfStockProducts(FragranceXProductsList productsList)
        {
            List<WalmartInventoryDatum> productsToRemove = new();
            List<FragranceXProduct> productsToRemoveList = new();
            List<FragranceXProduct> products = new();

            try
            {
                products = productsList.products;

                productsToRemove = (from s in productsRepository.GetBySkuPrefix(FRAGRANCEXSKUPREFIX)
                                    where !products.Any(x => x.Upc == s.Sku)
                                    select s).ToList();

                foreach (WalmartInventoryDatum item in productsToRemove)
                {
                    FragranceXProduct fragranceXProduct = new();

                    fragranceXProduct.Upc = item.Sku!;

                    productsToRemoveList.Add(fragranceXProduct);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemoveList;
        }

        public List<FragranceXProduct> FilterProductsToRemove(FragranceXProductsList productsList)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceXProduct> productsToRemove = new();
            List<FragranceXProduct> productsToOverride = new();
            List<RestrictedBrand> restrictedBrands = new List<RestrictedBrand>();
            List<RestrictedSku> restrictedSku = new List<RestrictedSku>();
            List<WalmartInventoryDatum> productsRemoveSaveData = new();

            try
            {
                restrictedBrands = (from s in restrictedBrandsRepository.GetAll()
                                    where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == "SBA"))
                                    select s).ToList<RestrictedBrand>();

                restrictedSku = (from s in restrictedSkusRepository.GetAll()
                                 where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == "SBA"))
                                 select s).ToList<RestrictedSku>();

                productsToRemove = (from s in productsList.products
                                    where (restrictedSku.Any(x => x.Sku == s.Upc) || restrictedBrands.Any(x => x.BrandName == s.BrandName))
                                    select s).ToList<FragranceXProduct>();

                productsToOverride = (from s in productsList.products
                                      where productsRepository.GetBySkuPrefix(TPSSKUPREFIX).Any(m =>  m.Sku == s.Upc)
                                      select s).ToList<FragranceXProduct>();

                productsRemoveSaveData = (from s in productsRepository.GetBySkuPrefix(FRAGRANCEXSKUPREFIX)
                                          where productsToRemove.Any(m => m.Upc == s.Sku)
                                          select s).ToList<WalmartInventoryDatum>();

                walmartInventoryRepository.DeleteMultiple(productsRemoveSaveData);

                walmartInventoryRepository.Save();

                productsToRemove.AddRange(productsToOverride);
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<FragranceXProduct> FilterProductsToProcess(FragranceXProductsList productsList, List<FragranceXProduct> removedProducts, List<FragranceXProduct> outOfStockProducts)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceXProduct> productsToProcess = new();
            List<FragranceXProduct> productsToSave = new();
            List<WalmartInventoryDatum> productsAddedSaveData = new();

            try
            {
                productsToProcess = (from s in productsList.products
                                    where !(removedProducts.Any(x => x.Upc == s.Upc) || outOfStockProducts.Any(x => x.Upc == s.Upc))
                                    select s).ToList<FragranceXProduct>();

                productsToSave = (from s in productsToProcess
                                where !productsRepository.GetAll().Any(m => m.Sku == s.Upc)
                                select s).ToList<FragranceXProduct>();

                foreach (FragranceXProduct product in productsToSave)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new();

                    walmartInventoryDatum.SkuPrefix = FRAGRANCEXSKUPREFIX;
                    walmartInventoryDatum.Sku = product.Upc;
                    walmartInventoryDatum.BrandName = product.BrandName;
                    walmartInventoryDatum.IsShippingMapped = false;

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
                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

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
                        fullSku = FRAGRANCEXSKUPREFIX + productData.Upc;
                        vendor = productData.BrandName;
                        mainTitle = productData.ProductName + " by " + vendor;

                        mpitem.Orderable.sku = fullSku;
                        mpitem.Orderable.productIdentifiers.productIdType = "GTIN";
                        mpitem.Orderable.productIdentifiers.productId = sku.PadLeft(14, '0');

                        mpitem.Orderable.productName = mainTitle;
                        mpitem.Orderable.brand = vendor;
                        mpitem.Orderable.price = applicationState.GetMarkedUpPrice(sku, productData.WholesalePriceUSD.ToString(), STORENAME.WALMART);
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

        public List<string> FormatSourceProductsInventoryData(List<FragranceXProduct> productsList, List<FragranceXProduct> outOfStockList, bool markOutOfStock)
        {
            WalmartInventoryRequestModel walmartInventoryRequestModel = new();
            IEnumerable<FragranceXProduct[]> thePerfumeSpotProductsMultiLists;
            List<string> productsData = new List<string>();

            try
            {
                productsList.AddRange(outOfStockList);

                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartInventoryRequestModel.inventoryHeader.version = "1.5";

                foreach (FragranceXProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartInventoryRequestModel.inventory.Clear();

                    foreach (FragranceXProduct productData in productsSingleList)
                    {
                        Inventory inventory = new();
                        Shipnode shipnode = new();

                        inventory.sku = FRAGRANCEXSKUPREFIX + productData.Upc;

                        shipnode.shipNode = FULFILLMENTCENTERID;
                        shipnode.quantity.unit = "EACH";
                        shipnode.quantity.amount = outOfStockList.Where(m => m.Upc == productData.Upc).ToList().Count > 0 ? 0 : Convert.ToInt32(productData.QuantityAvailable);

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

        public List<string> FormatShippingTemplateMappingData(List<FragranceXProduct> productsList)
        {
            WalmartShippingTemplateMapping walmartShippingTemplate = new();
            IEnumerable<FragranceXProduct[]> thePerfumeSpotProductsMultiLists;
            List<FragranceXProduct> productsListToMap;
            List<string> productsData = new List<string>();
            List<WalmartInventoryDatum> walmartInventoryDatumList = new();
            WalmartInventoryDataRepository walmartInventoryRepository = new();

            try
            {
                productsListToMap = (from s in productsList
                                     where (productsRepository.GetBySkuPrefix(FRAGRANCEXSKUPREFIX).Any(m => m.Sku == s.Upc && m.IsShippingMapped == false))
                                     select s).ToList<FragranceXProduct>();

                thePerfumeSpotProductsMultiLists = productsListToMap.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartShippingTemplate.ItemFeedHeader.sellingChannel = "precisedelivery";
                walmartShippingTemplate.ItemFeedHeader.locale = "en";
                walmartShippingTemplate.ItemFeedHeader.version = "1.0";

                foreach (FragranceXProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartShippingTemplate.Item.Clear();

                    foreach (FragranceXProduct productData in productsSingleList)
                    {
                        ShippingTemplateItem shippingTemplateItem = new();
                        Precisedelivery precisedelivery = new();

                        precisedelivery.sku = FRAGRANCEXSKUPREFIX + productData.Upc;
                        precisedelivery.actionType = "Add";
                        precisedelivery.shippingTemplateId = SHIPPINGTEMPLATEID;
                        precisedelivery.fulfillmentCenterId = FULFILLMENTCENTERID;

                        shippingTemplateItem.PreciseDelivery = precisedelivery;

                        walmartShippingTemplate.Item.Add(shippingTemplateItem);
                    }

                    productsData.Add(JsonConvert.SerializeObject(walmartShippingTemplate));
                }

                walmartInventoryDatumList = (from s in productsRepository.GetBySkuPrefix(FRAGRANCEXSKUPREFIX)
                                             where productsListToMap.Any(m => m.Upc == s.Sku)
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
