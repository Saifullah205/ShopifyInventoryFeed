using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync
{
    public partial class WalmartFeedStatusDetails : Form
    {
        string feedJsonResponse;
        string feedTypeSelected;
        ApplicationState applicationState;
        WalmartFeedResponseModel walmartFeedResponseModel = new();
        DataTable mainGridDataSource = new();

        public WalmartFeedStatusDetails(string feedResponse, string feedType)
        {
            InitializeComponent();

            feedResponse = File.ReadAllText("D:\\Clients_Work\\20220827_tdog5116\\Walmart\\SampleFeedResponse.txt");

            applicationState = ApplicationState.GetState;
            feedJsonResponse = feedResponse;
            feedTypeSelected = feedType;

            try
            {
                walmartFeedResponseModel = JsonConvert.DeserializeObject<WalmartFeedResponseModel>(feedJsonResponse)!;

                this.lblFeedTypeValue.Text = feedTypeSelected;
                this.lblFeedStatusValue.Text = walmartFeedResponseModel.feedStatus;

                if(walmartFeedResponseModel.feedStatus == "ERROR")
                {
                    showTextResponse(JsonConvert.SerializeObject(walmartFeedResponseModel.ingestionErrors));

                    return;
                }

                RefreshMainGrid();
            }
            catch (Exception)
            {
                showTextResponse(feedJsonResponse);
            }
        }

        private void showTextResponse(string message)
        {
            try
            {
                this.tableLayoutPanel1.Visible = false;

                TextBox textBox = new TextBox();

                textBox.Multiline = true;
                textBox.Location = new Point(0, 0);
                textBox.Size = new Size(800, 451);
                textBox.ReadOnly= true;

                textBox.Text = message;

                this.Controls.Add(textBox);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshMainGrid()
        {
            FixedPriceRespsitory fixedPriceRespsitory = new();

            try
            {
                mainGridDataSource.Columns.Add("SKU");
                mainGridDataSource.Columns.Add("Status");
                mainGridDataSource.Columns.Add("Error");

                foreach (Itemingestionstats item in walmartFeedResponseModel.itemDetails.itemIngestionStatus)
                {
                    DataRow dataRow = mainGridDataSource.NewRow();

                    dataRow["SKU"] = item.sku;
                    dataRow["Status"] = item.ingestionStatus;
                    dataRow["Error"] = item.ingestionErrors;

                    mainGridDataSource.Rows.Add(dataRow);
                }

                this.dgvDetails.DataSource= mainGridDataSource;
                this.dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void txtStatusSearch_TextChanged(object sender, EventArgs e)
        {
            List<Itemingestionstats> itemingestionstats = new();

            try
            {
                if (!string.IsNullOrEmpty(txtStatusSearch.Text))
                {
                    itemingestionstats = walmartFeedResponseModel.itemDetails.itemIngestionStatus.ToList().Where(m => m.ingestionStatus.Contains(txtStatusSearch.Text.ToUpper()) || m.sku.Contains(txtStatusSearch.Text.ToUpper())).ToList<Itemingestionstats>();
                }
                else
                {
                    itemingestionstats = walmartFeedResponseModel.itemDetails.itemIngestionStatus.ToList();
                }

                mainGridDataSource.Rows.Clear();

                foreach (Itemingestionstats item in itemingestionstats)
                {
                    DataRow dataRow = mainGridDataSource.NewRow();

                    dataRow["SKU"] = item.sku;
                    dataRow["Status"] = item.ingestionStatus;
                    dataRow["Error"] = item.ingestionErrors;

                    mainGridDataSource.Rows.Add(dataRow);
                }

                this.dgvDetails.DataSource = mainGridDataSource;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void dgvDetails_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvDetails.Rows[e.Row!.Index];
            FixedPrice shopifyFixedPrice = new();
            WalmartInventoryDataRepository walmartInventoryRepository = new();
            Itemingestionstats? itemingestionstats = new();
            string fullsku;
            string sku;
            string skuPrefix;

            try
            {
                fullsku = dataGridViewRow.Cells["SKU"].Value.ToString()!;

                sku = fullsku.Substring(3,fullsku.Length - 3);
                skuPrefix = fullsku.Substring(0, 3);

                if (feedTypeSelected == WALMARTFEEDTYPEPOST.SETUPITEM.ToString())
                {
                    walmartInventoryRepository.Delete(sku!);

                    walmartInventoryRepository.Save();
                }
                else if (feedTypeSelected == WALMARTFEEDTYPEPOST.MAPSHIPPINGTEMPLATE.ToString())
                {
                    WalmartInventoryDatum? walmartInventoryDatum = new ();

                    walmartInventoryDatum = walmartInventoryRepository.GetAll().Where(m => m.Sku == sku && m.SkuPrefix == skuPrefix).FirstOrDefault();

                    if (walmartInventoryDatum != null)
                    {
                        walmartInventoryDatum.IsShippingMapped = false;

                        walmartInventoryRepository.Update(walmartInventoryDatum);

                        walmartInventoryRepository.Save();
                    }                    
                }

                itemingestionstats = walmartFeedResponseModel.itemDetails.itemIngestionStatus.Where(m => m.sku == fullsku).FirstOrDefault();

                if (itemingestionstats != null)
                {
                    walmartFeedResponseModel.itemDetails.itemIngestionStatus.Remove(itemingestionstats);
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
    }
}
