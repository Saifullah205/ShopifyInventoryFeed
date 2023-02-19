using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.BusinessLogic.Walmart;
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
    public partial class WalmartFeedStatus : Form
    {
        List<WalmartFeedResponse> walmartFeedResponsesList = new ();

        private readonly IWalmartFeedResponseRepository walmartFeedResponse;
        private readonly ApplicationState applicationState;
        private readonly WalmartAPI walmartAPI;

        public WalmartFeedStatus()
        {
            InitializeComponent();

            walmartAPI = new();
            walmartFeedResponse = new WalmartFeedResponseRepository();
            applicationState = ApplicationState.GetState;

            RefreshWalmartFeedStatusGrid();
        }

        private void RefreshWalmartFeedStatusGrid()
        {
            try
            {
                walmartFeedResponsesList = walmartFeedResponse.GetAll().OrderByDescending(m => m.AddDate).Take(40).ToList<WalmartFeedResponse>();

                this.DGVFeedStatus.DataSource = applicationState.LinqToDataTable<WalmartFeedResponse>(walmartFeedResponsesList);

                this.DGVFeedStatus.Columns["Id"].Visible = false;
                this.DGVFeedStatus.Columns["EcomStoreId"].Visible = false;
                this.DGVFeedStatus.Columns["EcomStore"].Visible = false;

                this.DGVFeedStatus.Columns["FeedID"].HeaderText = "Feed ID";
                this.DGVFeedStatus.Columns["AddDate"].HeaderText = "Feed Time";

                this.DGVFeedStatus.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void DGVFeedStatus_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            String selectedFeedId = string.Empty;

            try
            {
                selectedFeedId = DGVFeedStatus.Rows[e.RowIndex].Cells["FeedId"].Value.ToString()!;

                txtFeedResponse.Text = walmartAPI.GetWalmartFeedResponse(selectedFeedId,chkIncludeDetails.Checked);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtFeedResponse.Clear();
            chkIncludeDetails.Checked = false;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            walmartAPI.FormatWalmartFeedResponse(txtFeedResponse.Text);
        }

        private void chkIncludeDetails_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                btnPreview.Enabled = !chkIncludeDetails.Checked;

                if (!chkIncludeDetails.Checked)
                {
                    txtFeedResponse.Clear();
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
