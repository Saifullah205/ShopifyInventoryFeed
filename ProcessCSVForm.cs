using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.BusinessLogic.Shopify;
using ShopifyInventorySync.BusinessLogic.Vendors;
using ShopifyInventorySync.BusinessLogic.Walmart;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync
{
    public partial class ProcessCSVForm : Form
    {
        DataTable productsDataTable = new ();
        int selectedAPI = (int)APITYPE.TPS;
        decimal totalProductsToProcessCount = 0;
        decimal progressBarIncrementValue = 0;
        decimal progressBarValue = 0;
        decimal shopifyprogressIndex = 0;
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
            try
            {
                if (applicationState.selectedStore == (int)STORENAME.SHOPIFY)
                {
                    if (selectedAPI == (int)APITYPE.TPS)
                    {
                        ProcessShopifyThePerfumeSpotProducts();
                    }
                    else if (selectedAPI == (int)APITYPE.FRAGRANCEX)
                    {
                        ProcessShopifyFragranceXProducts();
                    }
                    else if (selectedAPI == (int)APITYPE.FRAGRANCENET)
                    {
                        ProcessShopifyFragranceNetProducts();
                    }
                }
                else if (applicationState.selectedStore == (int)STORENAME.WALMART)
                {

                    WalmartFeedTypeForm walmartFeedType = new();

                    walmartFeedType.ShowDialog();

                    if (walmartFeedType.selectionCancelled)
                    {
                        return;
                    }
                    else
                    {
                        EnableApplicationMainControls(false);

                        if (selectedAPI == (int)APITYPE.TPS)
                        {
                            ProcessWalmartThePerfumeSpotProducts(walmartFeedType.selectedFeedType);
                        }
                        else if (selectedAPI == (int)APITYPE.FRAGRANCEX)
                        {
                            ProcessWalmartFragranceXProducts(walmartFeedType.selectedFeedType);
                        }
                        else if (selectedAPI == (int)APITYPE.FRAGRANCENET)
                        {
                            ProcessWalmartFragranceNetProducts(walmartFeedType.selectedFeedType);
                        }

                        EnableApplicationMainControls(true);
                    }
                }
            }
            catch (Exception ex)
            {
                EnableApplicationMainControls(true);

                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        #region WALMART

        #region MenuItems

        private void markupSettingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenMarkupSettingsWindow(STORENAME.WALMART);
        }

        private void restrictedBrandsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenRestrictedBrandsWindow(STORENAME.WALMART);
        }

        private void restrictedSKUsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenRestrictedSKUsWindow(STORENAME.WALMART);
        }

        private void fixedPricesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFixedPricesWindow(STORENAME.WALMART);
        }

        private void loadThePerfumeSpotProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(STORENAME.WALMART);

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

        private async void ProcessWalmartThePerfumeSpotProducts(WALMARTFEEDTYPEPOST actionType)
        {
            WalmartThePerfumeSpot clientAPI = new();
            WalmartAPI walmartAPI = new();
            List<ThePerfumeSpotProduct> outOfStockProducts;
            List<ThePerfumeSpotProduct> productsToDelete;
            List<ThePerfumeSpotProduct> productsToProcess;
            List<string> walmartProductsToPostData;
            List<string> inStockProductsToPostData;
            List<string> shippingTemplateMappingToPostData;

            try
            {
                outOfStockProducts = clientAPI.FilterOutOfStockProducts(thePerfumeSpotProductsList);
                productsToDelete = clientAPI.FilterProductsToRemove(thePerfumeSpotProductsList);
                productsToProcess = clientAPI.FilterProductsToProcess(thePerfumeSpotProductsList, productsToDelete);

                if (actionType == WALMARTFEEDTYPEPOST.SETUPITEM)
                {
                    walmartProductsToPostData = clientAPI.FormatSourceProductsData(productsToProcess);

                    if(walmartProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = walmartProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in walmartProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_ITEM));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.MAPSHIPPINGTEMPLATE)
                {
                    shippingTemplateMappingToPostData = clientAPI.FormatShippingTemplateMappingData(productsToProcess);

                    if (shippingTemplateMappingToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = shippingTemplateMappingToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in shippingTemplateMappingToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_SHIPPINGMAP));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.INVENTORYFEED)
                {
                    inStockProductsToPostData = clientAPI.FormatSourceProductsInventoryData(productsToProcess, outOfStockProducts, false);

                    if(inStockProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = inStockProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in inStockProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_INVENTORY));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.RETIRE)
                {

                    if(productsToDelete.Count > 0)
                    {
                        totalProductsToProcessCount = productsToDelete.Count; 
                        
                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (ThePerfumeSpotProduct product in productsToDelete)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => clientAPI.ProcessRetiredProductToWalmart(TPSSKUPREFIX + product.UPC!));
                        }
                    }
                    else
                    {
                        MessageBox.Show("No product found to delete.");
                    }
                }

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void ProcessWalmartFragranceXProducts(WALMARTFEEDTYPEPOST actionType)
        {
            WalmartFragranceX clientAPI = new();
            WalmartAPI walmartAPI = new();
            List<FragranceXProduct> outOfStockProducts;
            List<FragranceXProduct> productsToDelete;
            List<FragranceXProduct> productsToProcess;
            List<string> walmartProductsToPostData;
            List<string> inStockProductsToPostData;
            List<string> shippingTemplateMappingToPostData;

            try
            {
                outOfStockProducts = clientAPI.FilterOutOfStockProducts(fragranceXProducts);
                productsToDelete = clientAPI.FilterProductsToRemove(fragranceXProducts);
                productsToProcess = clientAPI.FilterProductsToProcess(fragranceXProducts, productsToDelete);

                if (actionType == WALMARTFEEDTYPEPOST.SETUPITEM)
                {
                    walmartProductsToPostData = clientAPI.FormatSourceProductsData(productsToProcess);

                    if (walmartProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = walmartProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in walmartProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_ITEM));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.MAPSHIPPINGTEMPLATE)
                {
                    shippingTemplateMappingToPostData = clientAPI.FormatShippingTemplateMappingData(productsToProcess);

                    if (shippingTemplateMappingToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = shippingTemplateMappingToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in shippingTemplateMappingToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_SHIPPINGMAP));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.INVENTORYFEED)
                {
                    inStockProductsToPostData = clientAPI.FormatSourceProductsInventoryData(productsToProcess, outOfStockProducts, false);

                    if (inStockProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = inStockProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in inStockProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_INVENTORY));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.RETIRE)
                {
                    if (productsToDelete.Count > 0)
                    {
                        totalProductsToProcessCount = productsToDelete.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (FragranceXProduct product in productsToDelete)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => clientAPI.ProcessRetiredProductToWalmart(FRAGRANCEXSKUPREFIX + product.Upc!));
                        }
                    }
                    else
                    {
                        MessageBox.Show("No product found to delete.");
                    }
                }

                await Task.Delay(100);

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void ProcessWalmartFragranceNetProducts(WALMARTFEEDTYPEPOST actionType)
        {
            WalmartFragranceNet clientAPI = new();
            WalmartAPI walmartAPI = new();
            List<FragranceNetProduct> outOfStockProducts;
            List<FragranceNetProduct> productsToDelete;
            List<FragranceNetProduct> productsToProcess;
            List<string> walmartProductsToPostData;
            List<string> inStockProductsToPostData;
            List<string> shippingTemplateMappingToPostData;

            try
            {
                outOfStockProducts = clientAPI.FilterOutOfStockProducts(fragranceNetProducts);
                productsToDelete = clientAPI.FilterProductsToRemove(fragranceNetProducts);
                productsToProcess = clientAPI.FilterProductsToProcess(fragranceNetProducts, productsToDelete);

                if (actionType == WALMARTFEEDTYPEPOST.SETUPITEM)
                {
                    walmartProductsToPostData = clientAPI.FormatSourceProductsData(productsToProcess);

                    if (walmartProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = walmartProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in walmartProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_ITEM));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.MAPSHIPPINGTEMPLATE)
                {
                    shippingTemplateMappingToPostData = clientAPI.FormatShippingTemplateMappingData(productsToProcess);

                    if (shippingTemplateMappingToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = shippingTemplateMappingToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in shippingTemplateMappingToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_SHIPPINGMAP));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.INVENTORYFEED)
                {
                    inStockProductsToPostData = clientAPI.FormatSourceProductsInventoryData(productsToProcess, outOfStockProducts, false);

                    if (inStockProductsToPostData.Count > 0)
                    {
                        totalProductsToProcessCount = inStockProductsToPostData.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (string feedData in inStockProductsToPostData)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => walmartAPI.ProcessProductToWalmart(feedData, WALMARTFEEDTYPE.MP_INVENTORY));
                        }
                    }
                }

                if (actionType == WALMARTFEEDTYPEPOST.RETIRE)
                {
                    if (productsToDelete.Count > 0)
                    {
                        totalProductsToProcessCount = productsToDelete.Count;

                        progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                        foreach (FragranceNetProduct product in productsToDelete)
                        {
                            IncrementProgressBar();

                            await Task.Run(() => clientAPI.ProcessRetiredProductToWalmart(FRAGRANCENETSKUPREFIX + product.upc!));
                        }
                    }
                    else
                    {
                        MessageBox.Show("No product found to delete.");
                    }
                }

                txtProcessedProducts.Text = applicationState.processingMessages;

                applicationState.ClearLogMessages();

                MessageBox.Show("Process Completed Successfully");

                ClearGridData();
            }
            catch (Exception)
            {
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

                progressBarValue = shopifyprogressIndex = 0;

                ProcessProgress.Value = (int)progressBarValue;

                lblProgressCount.Text = "0% Completed";
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void EnableApplicationMainControls(bool enable, bool showUnlimitedProgress = false)
        {
            try
            {
                btnClear.Enabled = enable;
                btnProcess.Enabled = enable;
                fileToolStripMenuItem.Enabled = enable;
                walmartToolStripMenuItem.Enabled = enable;
                settingsToolStripMenuItem1.Enabled = enable;
                helpToolStripMenuItem.Enabled = enable;

                if (showUnlimitedProgress)
                {
                    ProcessProgress.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    ProcessProgress.Style = ProgressBarStyle.Continuous;
                }                
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void OpenMarkupSettingsWindow(STORENAME sTORENAME)
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

        private void OpenRestrictedBrandsWindow(STORENAME sTORENAME)
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

        private void OpenRestrictedSKUsWindow(STORENAME sTORENAME)
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

        private void OpenFixedPricesWindow(STORENAME sTORENAME)
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
            ThePerfumeSpotAPI clientAPI = new();

            try
            {
                selectedAPI = (int)APITYPE.TPS;

                lblSelectedAPI.Text = "The Perfume Spot API";

                loadedDataGridView.DataSource = null;
                loadedDataGridView.Rows.Clear();
                thePerfumeSpotProductsList.products.Clear();

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

        private async void FetchTheFragranceNetProducts()
        {
            DataTable productsDataTable = new();
            FragranceNetAPI clientAPI = new ();

            try
            {
                EnableApplicationMainControls(false,true);

                ProgressBarLabelUpdate("Please Wait, Fetching data...");

                selectedAPI = (int)APITYPE.FRAGRANCENET;

                lblSelectedAPI.Text = "Fragrance Net API";

                loadedDataGridView.DataSource = null;
                loadedDataGridView.Rows.Clear();

                fragranceNetProducts = await clientAPI.FetchDataFromAPI();

                if (fragranceNetProducts.products.Count <= 0)
                {
                    MessageBox.Show("No data found");

                    return;
                }

                ProgressBarLabelUpdate("Formatting fetched data...");

                productsDataTable = applicationState.LinqToDataTable<FragranceNetProduct>(fragranceNetProducts.products as IEnumerable<FragranceNetProduct>);

                loadedDataGridView.DataSource = productsDataTable;

                btnProcess.Enabled = true;

                EnableApplicationMainControls(true);

                ProgressBarLabelUpdate("Process completed successfully.");
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                ProgressBarLabelUpdate("Unexpected error occurred.");

                throw;
            }
        }

        private async void FetchFragranceXProducts()
        {
            DataTable productsDataTable = new();
            FragranceXAPI clientAPI = new();

            try
            {
                EnableApplicationMainControls(false, true);

                selectedAPI = (int)APITYPE.FRAGRANCEX;

                lblSelectedAPI.Text = "Fragrance X API";

                fragranceXProducts.products.Clear();

                fragranceXProducts = await clientAPI.GetDataFromSource();

                if (fragranceXProducts.products.Count <= 0)
                {
                    MessageBox.Show("No data found");

                    return;
                }

                productsDataTable = applicationState.LinqToDataTable<FragranceXProduct>(fragranceXProducts.products as IEnumerable<FragranceXProduct>);

                loadedDataGridView.DataSource = productsDataTable;

                btnProcess.Enabled = true;

                EnableApplicationMainControls(true);

                ProgressBarLabelUpdate("Process completed successfully.");
            }
            catch (Exception)
            {
                EnableApplicationMainControls(true);

                ProgressBarLabelUpdate("Unexpected error occurred.");

                throw;
            }
        }

        private void IncrementShopifyProgressBar()
        {
            if (progressBarValue < 90)
            {
                progressBarValue += progressBarIncrementValue;
            }
            else
            {
                progressBarValue = 100;
            }

            ProgressBarLabelUpdate(shopifyprogressIndex.ToString() + "/" + totalProductsToProcessCount.ToString() + " Processed");

            ProcessProgress.Value = (int)progressBarValue;
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

            ProgressBarLabelUpdate(((int)progressBarValue).ToString() + "% Completed");

            ProcessProgress.Value = (int)progressBarValue;
        }

        private void ProgressBarLabelUpdate(string message)
        {
            lblProgressCount.Text = message;
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

        private void SetSelectedStore(STORENAME sTORENAME)
        {
            applicationState.selectedStore  = (int)sTORENAME;
        }

        #endregion

        #region SHOPIFY

        #region MenuItems

        private void fetchFragranceXProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(STORENAME.SHOPIFY);

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
                SetSelectedStore(STORENAME.SHOPIFY);

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
                SetSelectedStore(STORENAME.SHOPIFY);

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
            OpenMarkupSettingsWindow(STORENAME.SHOPIFY);
        }

        private void restrictedBrandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRestrictedBrandsWindow(STORENAME.SHOPIFY);
        }

        private void restrictedSKUsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRestrictedSKUsWindow(STORENAME.SHOPIFY);
        }

        private void fixedPricesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFixedPricesWindow(STORENAME.SHOPIFY);
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

                totalProductsToProcessCount = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                foreach (ThePerfumeSpotProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
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

                totalProductsToProcessCount = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                foreach (FragranceXProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
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

                totalProductsToProcessCount = fragranceNetProductsLists.Count + outOfStockProducts.Count;

                progressBarIncrementValue = (decimal)(100 / totalProductsToProcessCount);

                foreach (FragranceNetProductsList productsList in fragranceNetProductsLists)
                {
                    await Task.Run(() => clientAPI.ProcessProductToShopify(productsList));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
                }

                foreach (ShopifyInventoryDatum product in outOfStockProducts)
                {
                    await Task.Run(() => clientAPI.UpdateProductStockQuantity(product.Sku!, 0));

                    shopifyprogressIndex++;

                    IncrementShopifyProgressBar();
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

        private void fetchFragranceXProductsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(STORENAME.WALMART);

                FetchFragranceXProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void resetShippingMapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            List<WalmartInventoryDatum> walmartInventoryDatumList = new();

            try
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure to reset shipment mappings?", "Reset shipment mappings", MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    walmartInventoryDatumList = (from s in walmartInventoryRepository.GetAll()
                                                 select s).ToList<WalmartInventoryDatum>();

                    walmartInventoryDatumList.ForEach(c => c.IsShippingMapped = false);

                    walmartInventoryRepository.UpdateMultiple(walmartInventoryDatumList);

                    walmartInventoryRepository.Save();

                    MessageBox.Show("Shipment mappings reset successfully.");
                }                
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void fetchFragranceNetProductsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                SetSelectedStore(STORENAME.WALMART);

                FetchTheFragranceNetProducts();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
    }
}