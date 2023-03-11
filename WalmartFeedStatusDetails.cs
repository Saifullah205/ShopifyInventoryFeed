using Newtonsoft.Json;
using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}
