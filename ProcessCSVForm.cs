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
using System.Collections;
using ShopifyInventorySync.Repositories;
using System.Collections.Generic;

namespace ShopifyInventorySync
{
    public partial class ProcessCSVForm : Form
    {
        DataTable productsDataTable = new ();
        string selectedCSVFile = string.Empty;
        int selectedAPI = (int)SharedData.APIType.TPS;
        decimal progressBarTotalValue = 0;
        decimal progressBarIncrementValue = 0;
        decimal progressBarValue = 0;
        List<string> ProductNames = new List<string> ();
        List<ShopifyInventoryDatum> shopifyProductsToRemove = new List<ShopifyInventoryDatum>();
        List<ThePerfumeSpotProduct> productsDataList = new List<ThePerfumeSpotProduct> ();
        List<ShopifyInventoryDatum> shopifyProductsData = new List<ShopifyInventoryDatum>();
        List<MarkUpPrice> markUpPricesList = new List<MarkUpPrice>();
        List<RestrictedBrand> restrictedBrandsList = new ();
        List<RestrictedSku> restrictedSkusList = new();
        List<ShopifyFixedPrice> shopifyFixedPricesList = new();
        ThePerfumeSpotProductsList thePerfumeSpotProductsList = new ();
        FragranceNetProductsList fragranceNetProducts = new ();
        FragranceXProductsList fragranceXProducts = new();

        ApplicationState applicationState;

        public ProcessCSVForm()
        {
            InitializeComponent();

            txtProcessedProducts.ScrollBars = ScrollBars.Vertical;

            lblSelectedAPI.Text = "CSV API";
            lblProgressCount.Text = "0% Completed";

            applicationState = ApplicationState.GetState;
        }

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (selectedAPI == (int)SharedData.APIType.TPS)
            {
                ProcessThePerfumeSpotProducts();
            }
            else if (selectedAPI == (int)SharedData.APIType.FragranceX)
            {
                ProcessFragranceXProducts();
            }
            else if (selectedAPI == (int)SharedData.APIType.FragranceNet)
            {
                ProcessFragranceNetProducts();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable productsDataTable = new();
            ThePerfumeSpotAPI clientAPI = new ();

            try
            {
                selectedAPI = (int)SharedData.APIType.TPS;

                lblSelectedAPI.Text = "The Perfume Spot API";

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
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private async void ProcessThePerfumeSpotProducts()
        {
            ThePerfumeSpotAPI clientAPI = new();
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

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form applicationSettingsForm;

            try
            {
                applicationSettingsForm = new ApplicationSettingsForm();

                applicationSettingsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

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

        private void restrictedBrandsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestrictedBrandsForm restrictedBrandsForm;

            try
            {
                restrictedBrandsForm = new();

                restrictedBrandsForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

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
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void fetchFragranceXProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable productsDataTable = new();
            FragranceXAPI clientAPI = new ();

            try
            {
                selectedAPI = (int)SharedData.APIType.FragranceNet;

                lblSelectedAPI.Text = "Fragrance Net API";

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
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private async void ProcessFragranceXProducts()
        {
            FragranceXAPI clientAPI = new();
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

        private void fixedPricesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FixedPricesForm fixedPricesForm;

            try
            {
                fixedPricesForm = new();

                fixedPricesForm.ShowDialog();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

		private void fetchFragranceNetProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable productsDataTable = new();
            FragranceNetAPI clientAPI = new FragranceNetAPI();

            try
            {
                selectedAPI = (int)SharedData.APIType.FragranceNet;

                lblSelectedAPI.Text = "Fragrance Net API";

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
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private async void ProcessFragranceNetProducts()
        {
            FragranceNetAPI clientAPI = new ();
            List<ShopifyInventoryDatum> outOfStockProducts = new();
            List<FragranceNetProductsList> fragranceNetProductsLists = new();
            List<ShopifyInventoryDatum> fragranceNetAPIProducts = new ();
            List<Task> tasks = new ();

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
    }
}