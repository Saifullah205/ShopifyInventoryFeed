using CsvHelper;
using RestSharp;
using ShopifyInventorySync.Models;
using System.Data;
using System.Globalization;
using System.Reflection;
using static ShopifyInventorySync.ThePerfumeSpotProduct;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.BusinessLogic.Shopify;
using System.Collections;
using ShopifyInventorySync.Repositories;
using System.Collections.Generic;
using ShopifyInventorySync.BusinessLogic.Walmart;

namespace ShopifyInventorySync
{
    public partial class ProcessCSVForm : Form
    {
        DataTable productsDataTable = new ();
        int selectedAPI = (int)GlobalConstants.APITYPE.TPS;
        int selectedStore = (int)GlobalConstants.STORENAME.SHOPIFY;
        decimal progressBarTotalValue = 0;
        decimal progressBarIncrementValue = 0;
        decimal progressBarValue = 0;
        ThePerfumeSpotProductsList thePerfumeSpotProductsList = new ();
        FragranceNetProductsList fragranceNetProducts = new ();
        FragranceXProductsList fragranceXProducts = new();

        ApplicationState applicationState;

        public ProcessCSVForm()
        {
            InitializeComponent();

            txtProcessedProducts.ScrollBars = ScrollBars.Vertical;

            lblSelectedAPI.Text = "The Perfume Spot API";
            lblProgressCount.Text = "0% Completed";

            applicationState = ApplicationState.GetState;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedStore == (int)GlobalConstants.STORENAME.SHOPIFY)
            {
                if (selectedAPI == (int)GlobalConstants.APITYPE.TPS)
                {
                    ProcessShopifyThePerfumeSpotProducts();
                }
                else if (selectedAPI == (int)GlobalConstants.APITYPE.FRAGRANCEX)
                {
                    ProcessShopifyFragranceXProducts();
                }
                else if (selectedAPI == (int)GlobalConstants.APITYPE.FRAGRANCENET)
                {
                    ProcessShopifyFragranceNetProducts();
                }
            }
            else if (selectedStore == (int)GlobalConstants.STORENAME.WALMART)
            {

                WalmartFeedTypeForm walmartFeedType = new ();

                walmartFeedType.ShowDialog();
                                
                if (selectedAPI == (int)GlobalConstants.APITYPE.TPS)
                {
                    ProcessWalmartThePerfumeSpotProducts(walmartFeedType.selectedFeedType);
                }
            }
        }

        #region WALMART

        #region MenuItems

        private void markupSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenMarkupSettingsWindow(GlobalConstants.STORENAME.WALMART);
        }

        private void restrictedBrandsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenRestrictedBrandsWindow(GlobalConstants.STORENAME.WALMART);
        }

        private void restrictedSKUsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenRestrictedSKUsWindow(GlobalConstants.STORENAME.WALMART);
        }

        private void fixedPricesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFixedPricesWindow(GlobalConstants.STORENAME.WALMART);
        }

        private void loadThePerfumeSpotProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(GlobalConstants.STORENAME.WALMART);

                BrowseThePerfumeSpotProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region ProcessProducts

        private async void ProcessWalmartThePerfumeSpotProducts(GlobalConstants.WALMARTFEEDTYPEPOST actionType)
        {
            WalmartThePerfumeSpot clientAPI = new();
            List<WalmartInventoryDatum> outOfStockProducts = new();
            List<WalmartInventoryDatum> inStockProducts = new();
            List<ThePerfumeSpotProduct> productsToDelete = new();
            List<ThePerfumeSpotProduct> productsToProcess = new();
            List<string> walmartProductsToPostData;
            List<string> inStockProductsToPostData;
            List<string> outOfStockProductsToPostData;

            try
            {
                EnableApplicationMainControls(false);

                outOfStockProducts = clientAPI.FilterOutOfStockProducts(thePerfumeSpotProductsList);
                productsToDelete = clientAPI.FilterProductsToRemove(thePerfumeSpotProductsList);
                productsToProcess = clientAPI.FilterProductsToProcess(thePerfumeSpotProductsList, productsToDelete, outOfStockProducts);
                inStockProducts = clientAPI.PrepareInStockProductsQtyToProcess(productsToProcess);

                if (actionType == GlobalConstants.WALMARTFEEDTYPEPOST.SETUPITEM)
                {
                    walmartProductsToPostData = clientAPI.FormatSourceProductsData(productsToProcess);

                    if(walmartProductsToPostData.Count > 0)
                    {
                        progressBarTotalValue = walmartProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                        foreach (string feedData in walmartProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => clientAPI.ProcessProductToWalmart(feedData, GlobalConstants.WALMARTFEEDTYPE.MP_ITEM));
                        }
                    }                    
                }

                if (actionType == GlobalConstants.WALMARTFEEDTYPEPOST.INVENTORYFEED)
                {
                    inStockProductsToPostData = clientAPI.FormatSourceProductsInventoryData(inStockProducts, false);

                    if(inStockProductsToPostData.Count > 0)
                    {
                        progressBarTotalValue = inStockProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                        foreach (string feedData in inStockProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => clientAPI.ProcessProductToWalmart(feedData, GlobalConstants.WALMARTFEEDTYPE.MP_INVENTORY));
                        }
                    }                    
                }

                if (actionType == GlobalConstants.WALMARTFEEDTYPEPOST.OUTOFSTOCK)
                {
                    outOfStockProductsToPostData = clientAPI.FormatSourceProductsInventoryData(outOfStockProducts, true);

                    if(outOfStockProductsToPostData.Count > 0)
                    {
                        progressBarTotalValue = outOfStockProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                        foreach (string feedData in outOfStockProductsToPostData)
                        {
                            IncrementProgressBar();

                            //await Task.Run(() => clientAPI.ProcessProductToWalmart(feedData, GlobalConstants.WALMARTFEEDTYPE.MP_INVENTORY));
                        }
                    }                    
                }

                if (actionType == GlobalConstants.WALMARTFEEDTYPEPOST.RETIRE)
                {
                    progressBarTotalValue = productsToDelete.Count;

                    progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                    foreach (ThePerfumeSpotProduct product in productsToDelete)
                    {
                        IncrementProgressBar();

                        //await Task.Run(() => clientAPI.ProcessRetiredProductToWalmart(GlobalConstants.tpsSKUPrefix + product.UPC!));
                    }
                }                    

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();

                EnableApplicationMainControls(true);
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                throw;
            }
        }

        #endregion

        #endregion

        #region GENERIC_METHODS

        private void ClearGridData()
        {
            try
            {
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
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void EnableApplicationMainControls(bool enable)
        {
            try
            {
                btnClear.Enabled = enable;
                btnProcess.Enabled = enable;
                fileToolStripMenuItem.Enabled = enable;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OpenMarkupSettingsWindow(GlobalConstants.STORENAME sTORENAME)
        {
            MarkUpPricesForm markUpPricesForm;

            try
            {
                markUpPricesForm = new(sTORENAME);

                markUpPricesForm.ShowDialog();

                applicationState.RefreshMarkUPPricesList();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void OpenRestrictedBrandsWindow(GlobalConstants.STORENAME sTORENAME)
        {
            RestrictedBrandsForm restrictedBrandsForm;

            try
            {
                restrictedBrandsForm = new(sTORENAME);

                restrictedBrandsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void OpenRestrictedSKUsWindow(GlobalConstants.STORENAME sTORENAME)
        {
            RestrictedSkusForm restrictedSkusForm;

            try
            {
                restrictedSkusForm = new(sTORENAME);

                restrictedSkusForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void OpenFixedPricesWindow(GlobalConstants.STORENAME sTORENAME)
        {
            FixedPricesForm fixedPricesForm;

            try
            {
                fixedPricesForm = new(sTORENAME);

                fixedPricesForm.ShowDialog();

                applicationState.RefreshFixedPricesList();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void BrowseThePerfumeSpotProducts()
        {
            DataTable productsDataTable = new();
            ShopifyThePerfumeSpot clientAPI = new();

            try
            {
                selectedAPI = (int)GlobalConstants.APITYPE.TPS;

                lblSelectedAPI.Text = "The Perfume Spot API";

                loadedDataGridView.DataSource = null;
                loadedDataGridView.Rows.Clear();

                thePerfumeSpotProductsList = clientAPI.GetDataFromSource();

                if (thePerfumeSpotProductsList.products.Count <= 0)
                {
                    MessageBox.Show("No data found");

                    return;
                }

                productsDataTable = applicationState.LinqToDataTable<ThePerfumeSpotProduct>(thePerfumeSpotProductsList.products as IEnumerable<ThePerfumeSpotProduct>);

                loadedDataGridView.DataSource = productsDataTable;

                btnProcess.Enabled = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void FetchTheFragranceNetProducts()
        {
            DataTable productsDataTable = new();
            ShopifyFragranceNet clientAPI = new ShopifyFragranceNet();

            try
            {
                selectedAPI = (int)GlobalConstants.APITYPE.FRAGRANCENET;

                lblSelectedAPI.Text = "Fragrance Net API";

                loadedDataGridView.DataSource = null;
                loadedDataGridView.Rows.Clear();

                fragranceNetProducts = clientAPI.GetDataFromSource();

                if (fragranceNetProducts.products.Count <= 0)
                {
                    MessageBox.Show("No data found");

                    return;
                }

                productsDataTable = applicationState.LinqToDataTable<FragranceNetProduct>(fragranceNetProducts.products as IEnumerable<FragranceNetProduct>);

                loadedDataGridView.DataSource = productsDataTable;

                btnProcess.Enabled = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void FetchFragranceXProducts()
        {
            DataTable productsDataTable = new();
            ShopifyFragranceX clientAPI = new();

            try
            {
                selectedAPI = (int)GlobalConstants.APITYPE.FRAGRANCEX;

                lblSelectedAPI.Text = "Fragrance X API";

                fragranceXProducts = clientAPI.GetDataFromSource();

                if (fragranceXProducts.products.Count <= 0)
                {
                    MessageBox.Show("No data found");

                    return;
                }

                productsDataTable = applicationState.LinqToDataTable<FragranceXProduct>(fragranceXProducts.products as IEnumerable<FragranceXProduct>);

                loadedDataGridView.DataSource = productsDataTable;

                btnProcess.Enabled = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void IncrementProgressBar()
        {
            if (progressBarValue < 90)
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

        private void settingsToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Form applicationSettingsForm;

            try
            {
                applicationSettingsForm = new ApplicationSettingsForm();

                applicationSettingsForm.ShowDialog();

                applicationState.RefreshApplicationSettings();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form aboutForm;

            try
            {
                aboutForm = new AboutForm();

                aboutForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
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
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void SetSelectedStore(GlobalConstants.STORENAME sTORENAME)
        {
            selectedStore = (int)sTORENAME;
        }

        #endregion

        #region SHOPIFY

        #region MenuItems

        private void fetchFragranceXProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(GlobalConstants.STORENAME.SHOPIFY);

                FetchFragranceXProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void fetchFragranceNetProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(GlobalConstants.STORENAME.SHOPIFY);

                FetchTheFragranceNetProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(GlobalConstants.STORENAME.SHOPIFY);

                BrowseThePerfumeSpotProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void markupSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMarkupSettingsWindow(GlobalConstants.STORENAME.SHOPIFY);
        }

        private void restrictedBrandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRestrictedBrandsWindow(GlobalConstants.STORENAME.SHOPIFY);
        }

        private void restrictedSKUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRestrictedSKUsWindow(GlobalConstants.STORENAME.SHOPIFY);
        }

        private void fixedPricesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFixedPricesWindow(GlobalConstants.STORENAME.SHOPIFY);
        }

        #endregion

        #region ProcessProducts

        private async void ProcessShopifyThePerfumeSpotProducts()
        {
            ShopifyThePerfumeSpot clientAPI = new();
            List<ShopifyInventoryDatum> outOfStockProducts = new();
            List<ThePerfumeSpotProductsList> fragranceNetProductsLists = new();
            List<ShopifyInventoryDatum> fragranceNetAPIProducts = new();
            List<Task> tasks = new();

            try
            {
                EnableApplicationMainControls(false);

                outOfStockProducts = clientAPI.FilterRemovedProducts(thePerfumeSpotProductsList);

                fragranceNetProductsLists = clientAPI.FormatSourceProductsData(thePerfumeSpotProductsList);

                progressBarTotalValue = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                foreach (ThePerfumeSpotProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    IncrementProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    IncrementProgressBar();
                }

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();

                EnableApplicationMainControls(true);
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                throw;
            }
        }

        private async void ProcessShopifyFragranceXProducts()
        {
            ShopifyFragranceX clientAPI = new();
            List<ShopifyInventoryDatum> outOfStockProducts = new();
            List<FragranceXProductsList> fragranceNetProductsLists = new();
            List<ShopifyInventoryDatum> fragranceNetAPIProducts = new();
            List<Task> tasks = new();

            try
            {
                EnableApplicationMainControls(false);

                outOfStockProducts = clientAPI.FilterRemovedProducts(fragranceXProducts);

                fragranceNetProductsLists = clientAPI.FormatSourceProductsData(fragranceXProducts);

                progressBarTotalValue = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                foreach (FragranceXProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    IncrementProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    IncrementProgressBar();
                }

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();

                EnableApplicationMainControls(true);
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                throw;
            }
        }

        private async void ProcessShopifyFragranceNetProducts()
        {
            ShopifyFragranceNet clientAPI = new();
            List<ShopifyInventoryDatum> outOfStockProducts = new();
            List<FragranceNetProductsList> fragranceNetProductsLists = new();
            List<ShopifyInventoryDatum> fragranceNetAPIProducts = new();
            List<Task> tasks = new();

            try
            {
                EnableApplicationMainControls(false);

                outOfStockProducts = clientAPI.FilterRemovedProducts(fragranceNetProducts);

                fragranceNetProductsLists = clientAPI.FormatSourceProductsData(fragranceNetProducts);

                progressBarTotalValue = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / progressBarTotalValue);

                foreach (FragranceNetProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    IncrementProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    IncrementProgressBar();
                }

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();

                EnableApplicationMainControls(true);
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                throw;
            }
        }

        #endregion

        #endregion

        private void getFeedStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                WalmartFeedStatus walmartFeedStatus = new();

                walmartFeedStatus.ShowDialog();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}