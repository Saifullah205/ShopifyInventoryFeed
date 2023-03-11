using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.BusinessLogic.Walmart;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;

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
                walmartFeedResponsesList = walmartFeedResponse.GetAll().OrderByDescending(m => m.AddDate).Take(50).ToList<WalmartFeedResponse>();

                this.DGVFeedStatus.DataSource = applicationState.LinqToDataTable<WalmartFeedResponse>(walmartFeedResponsesList);

                this.DGVFeedStatus.Columns["EcomStoreId"].Visible = false;
                this.DGVFeedStatus.Columns["EcomStore"].Visible = false;
                this.DGVFeedStatus.Columns["FeedId"].Visible = false;

                this.DGVFeedStatus.Columns["Id"].HeaderText = "Feed No";
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
            String selectedFeedType = string.Empty;
            string walmartFeedResponse;
            WalmartFeedStatusDetails walmartFeedStatusDetails;

            try
            {
                this.DGVFeedStatus.Enabled= false;

                selectedFeedId = DGVFeedStatus.Rows[e.RowIndex].Cells["FeedId"].Value.ToString()!;
                selectedFeedType = DGVFeedStatus.Rows[e.RowIndex].Cells["FeedType"].Value.ToString()!;

                walmartFeedResponse = walmartAPI.GetWalmartFeedResponse(selectedFeedId,true);

                walmartFeedStatusDetails = new(walmartFeedResponse, selectedFeedType);

                walmartFeedStatusDetails.ShowDialog();

                this.DGVFeedStatus.Enabled = true;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
    }
}
