using Newtonsoft.Json;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    internal class WalmartFragranceNet
    {
        ApplicationState applicationState;

        private readonly IWalmartFeedResponseRepository walmartFeedResponseRepository;
        private readonly IWalmartInventoryDataRepository productsRepository;
        private readonly IRestrictedBrandsRepository restrictedBrandsRepository;
        private readonly IRestrictedSkusRepository restrictedSkusRepository;
        private readonly IRestrictedTermsRepository restrictedTermsRepository;
        private readonly WalmartAPI walmartAPI;

        public WalmartFragranceNet()
        {
            applicationState = ApplicationState.GetState;
            walmartAPI = new();
            productsRepository = new WalmartInventoryDataRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();
            walmartFeedResponseRepository = new WalmartFeedResponseRepository();
            restrictedTermsRepository = new RestrictedTermsRepository();
        }

        public List<FragranceNetProduct> FilterOutOfStockProducts(FragranceNetProductsList productsList)
        {
            List<WalmartInventoryDatum> productsToRemove = new();
            List<FragranceNetProduct> productsToRemoveList = new();
            List<FragranceNetProduct> products = new();

            try
            {
                products = productsList.products;

                productsToRemove = (from s in productsRepository.GetBySkuPrefix(FRAGRANCENETSKUPREFIX)
                                    where !products.Any(x => x.upc == s.Sku)
                                    select s).ToList();

                foreach (WalmartInventoryDatum item in productsToRemove)
                {
                    FragranceNetProduct fNetproduct = new();

                    fNetproduct.upc = item.Sku!;

                    productsToRemoveList.Add(fNetproduct);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemoveList;
        }

        public List<FragranceNetProduct> FilterProductsToRemove(FragranceNetProductsList productsList)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceNetProduct> productsToRemove = new();
            List<RestrictedBrand> restrictedBrands = new List<RestrictedBrand>();
            List<RestrictedSku> restrictedSku = new List<RestrictedSku>();
            List<RestrictedTerm> restrictedTerms = new();
            List<WalmartInventoryDatum> productsRemoveSaveData = new();

            try
            {
                restrictedBrands = (from s in restrictedBrandsRepository.GetAll()
                                    where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == FRAGRANCENETSKUPREFIX))
                                    select s).ToList();

                restrictedSku = (from s in restrictedSkusRepository.GetAll()
                                 where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == FRAGRANCENETSKUPREFIX))
                                 select s).ToList();

                restrictedTerms = (from s in restrictedTermsRepository.GetAll()
                                   where (s.EcomStoreId == (int)STORENAME.WALMART && (s.ApiType == "ALL" || s.ApiType == FRAGRANCENETSKUPREFIX))
                                   select s).ToList();

                productsToRemove = (from s in productsList.products
                                    where (restrictedSku.Any(x => x.Sku == s.upc) || restrictedBrands.Any(x => x.BrandName!.Trim().ToUpper() == s.designer.Trim().ToUpper()) || restrictedTerms.Any(x => s.name.Trim().ToUpper().Contains(x.Term!.Trim().ToUpper())))
                                    select s).ToList();

                productsRemoveSaveData = (from s in productsRepository.GetBySkuPrefix(FRAGRANCENETSKUPREFIX)
                                          where productsToRemove.Any(m => m.upc == s.Sku)
                                          select s).ToList();

                walmartInventoryRepository.DeleteMultiple(productsRemoveSaveData);

                walmartInventoryRepository.Save();
            }
            catch (Exception)
            {
                throw;
            }

            return productsToRemove;
        }

        public List<FragranceNetProduct> FilterProductsToProcess(FragranceNetProductsList productsList, List<FragranceNetProduct> removedProducts)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<FragranceNetProduct> productsToProcess = new();
            List<FragranceNetProduct> productsToIgnore = new();
            List<FragranceNetProduct> productsToSave = new();
            List<WalmartInventoryDatum> productsToUpdate = new();
            List<WalmartInventoryDatum> productsAddedSaveData = new();
            List<WalmartInventoryDatum> allWalmartProducts = new();

            try
            {
                allWalmartProducts = productsRepository.GetAll().ToList();

                productsToIgnore = (from s in productsList.products
                                    where allWalmartProducts.Any(m => m.Sku == s.upc && m.SkuPrefix != FRAGRANCENETSKUPREFIX && m.Price < Convert.ToDecimal(s.fnetWholesalePrice))
                                    select s).ToList();

                productsToProcess = (from s in productsList.products
                                    where !(removedProducts.Any(x => x.upc == s.upc) || productsToIgnore.Any(x => x.upc == s.upc))
                                     select s).ToList();

                productsToSave = (from s in productsToProcess
                                where !allWalmartProducts.Any(m => m.Sku == s.upc)
                                select s).ToList();

                productsToUpdate = (from s in walmartInventoryRepository.GetAll()
                                    where productsToProcess.Any(x => x.upc == s.Sku)
                                    select s).ToList();

                foreach (FragranceNetProduct product in productsToSave)
                {
                    WalmartInventoryDatum walmartInventoryDatum = new()
                    {
                        SkuPrefix = FRAGRANCENETSKUPREFIX,
                        Sku = product.upc,
                        BrandName = product.designer.Trim(),
                        IsShippingMapped = false,
                        Price = Convert.ToDecimal(product.fnetWholesalePrice)
                    };

                    productsAddedSaveData.Add(walmartInventoryDatum);
                }

                productsToUpdate.ForEach(x =>
                {
                    x.Price = Convert.ToDecimal(productsToProcess.Where(m => m.upc == x.Sku).First().fnetWholesalePrice);
                    x.SkuPrefix = FRAGRANCENETSKUPREFIX;
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

        public List<string> FormatSourceProductsData(List<FragranceNetProduct> productsList)
        {
            string productName = string.Empty;
            string genderMale = string.Empty;
            string genderFemale = string.Empty;
            WalmartProductModel walmartProductModel = new();
            List<string> productsData = new List<string>();
            IEnumerable<FragranceNetProduct[]> thePerfumeSpotProductsMultiLists;

            try
            {
                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartProductModel.MPItemFeedHeader.sellingChannel = "marketplace";
                walmartProductModel.MPItemFeedHeader.processMode = "REPLACE";
                walmartProductModel.MPItemFeedHeader.subset = "EXTERNAL";
                walmartProductModel.MPItemFeedHeader.locale = "en";
                walmartProductModel.MPItemFeedHeader.version = "1.5";
                walmartProductModel.MPItemFeedHeader.subCategory = "office_other";

                foreach (FragranceNetProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartProductModel.MPItem.Clear();

                    foreach (FragranceNetProduct productData in productsSingleList)
                    {
                        Mpitem mpitem = new();
                        string sku = string.Empty;
                        string fullSku = string.Empty;
                        string vendor = string.Empty;
                        string mainTitle = string.Empty;
                        decimal calculatedCost = 0;

                        sku = productData.upc;
                        fullSku = FRAGRANCENETSKUPREFIX + productData.upc;
                        vendor = productData.designer.Trim();
                        mainTitle = productData.name;
                        calculatedCost = applicationState.GetMarkedUpPrice(sku, productData.fnetWholesalePrice.ToString(), STORENAME.WALMART);

                        mpitem.Orderable.sku = fullSku;
                        mpitem.Orderable.productIdentifiers.productIdType = "GTIN";
                        mpitem.Orderable.productIdentifiers.productId = sku.PadLeft(14, '0');

                        mpitem.Orderable.productName = mainTitle;
                        mpitem.Orderable.brand = vendor;
                        mpitem.Orderable.price = calculatedCost < Convert.ToDecimal(WALMARTMINPRICELEVEL) ? Convert.ToDecimal(WALMARTMINPRICELEVEL) : calculatedCost;
                        mpitem.Orderable.ShippingWeight = 0;
                        mpitem.Orderable.electronicsIndicator = "No";
                        mpitem.Orderable.batteryTechnologyType = "Does Not Contain a Battery";
                        mpitem.Orderable.chemicalAerosolPesticide = "No";
                        mpitem.Orderable.shipsInOriginalPackaging = "No";
                        mpitem.Orderable.startDate = "2019-01-01T08:00:00Z";
                        mpitem.Orderable.endDate = "2060-01-01T08:00:00Z";
                        mpitem.Orderable.MustShipAlone = "No";

                        mpitem.Visible.Office.shortDescription = "Storage";
                        mpitem.Visible.Office.mainImageUrl = productData.imageLarge;
                        mpitem.Visible.Office.productSecondaryImageURL = new List<string> { productData.imageLarge };
                        mpitem.Visible.Office.prop65WarningText = "None";
                        mpitem.Visible.Office.smallPartsWarnings = new List<string> { "0 - No warning applicable" };
                        mpitem.Visible.Office.compositeWoodCertificationCode = "1 - Does not contain composite wood";
                        mpitem.Visible.Office.keyFeatures = new List<string> { mainTitle };
                        mpitem.Visible.Office.manufacturer = productData.designer.Trim();
                        mpitem.Visible.Office.manufacturerPartNumber = productData.upc;

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

        public List<string> FormatSourceProductsInventoryData(List<FragranceNetProduct> productsList, List<FragranceNetProduct> outOfStockList, bool markOutOfStock)
        {
            WalmartInventoryRequestModel walmartInventoryRequestModel = new();
            IEnumerable<FragranceNetProduct[]> thePerfumeSpotProductsMultiLists;
            List<string> productsData = new List<string>();

            try
            {
                productsList.AddRange(outOfStockList);

                thePerfumeSpotProductsMultiLists = productsList.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartInventoryRequestModel.inventoryHeader.version = "1.5";

                foreach (FragranceNetProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartInventoryRequestModel.inventory.Clear();

                    foreach (FragranceNetProduct productData in productsSingleList)
                    {
                        Inventory inventory = new();
                        Shipnode shipnode = new();

                        inventory.sku = FRAGRANCENETSKUPREFIX + productData.upc;

                        shipnode.shipNode = FULFILLMENTCENTERID;
                        shipnode.quantity.unit = "EACH";
                        shipnode.quantity.amount = outOfStockList.Where(m => m.upc == productData.upc).ToList().Count > 0 ? 0 : Convert.ToInt32(productData.quantity);

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

        public List<string> FormatShippingTemplateMappingData(List<FragranceNetProduct> productsList)
        {
            WalmartShippingTemplateMapping walmartShippingTemplate = new();
            IEnumerable<FragranceNetProduct[]> thePerfumeSpotProductsMultiLists;
            List<FragranceNetProduct> productsListToMap;
            List<string> productsData = new List<string>();
            List<WalmartInventoryDatum> walmartInventoryDatumList = new();
            WalmartInventoryDataRepository walmartInventoryRepository = new();

            try
            {
                productsListToMap = (from s in productsList
                                     where (productsRepository.GetBySkuPrefix(FRAGRANCENETSKUPREFIX).Any(m => m.Sku == s.upc && m.IsShippingMapped == false))
                                     select s).ToList();

                thePerfumeSpotProductsMultiLists = productsListToMap.Chunk(Convert.ToInt32(WALMARTCHUNKSIZE));

                walmartShippingTemplate.ItemFeedHeader.sellingChannel = "precisedelivery";
                walmartShippingTemplate.ItemFeedHeader.locale = "en";
                walmartShippingTemplate.ItemFeedHeader.version = "1.0";

                foreach (FragranceNetProduct[] productsSingleList in thePerfumeSpotProductsMultiLists)
                {
                    walmartShippingTemplate.Item.Clear();

                    foreach (FragranceNetProduct productData in productsSingleList)
                    {
                        Precisedelivery precisedelivery = new()
                        {
                            sku = FRAGRANCENETSKUPREFIX + productData.upc,
                            actionType = "Add",
                            shippingTemplateId = SHIPPINGTEMPLATEID,
                            fulfillmentCenterId = FULFILLMENTCENTERID
                        };

                        ShippingTemplateItem shippingTemplateItem = new()
                        {
                            PreciseDelivery = precisedelivery
                        };

                        walmartShippingTemplate.Item.Add(shippingTemplateItem);
                    }

                    productsData.Add(JsonConvert.SerializeObject(walmartShippingTemplate));
                }

                walmartInventoryDatumList = (from s in productsRepository.GetBySkuPrefix(FRAGRANCENETSKUPREFIX)
                                             where productsListToMap.Any(m => m.upc == s.Sku)
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
