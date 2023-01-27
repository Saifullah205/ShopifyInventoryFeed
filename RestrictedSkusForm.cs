using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;

namespace ShopifyInventorySync
{
    public partial class RestrictedSkusForm : Form
    {
        CommonRepository commonRepository;
        IRestrictedSkusRepository restrictedSkusRepository;
        List<ClientApi> clientApis = new();
        List<RestrictedSku> restrictedSkusList = new List<RestrictedSku>();
        public GlobalConstants.STORENAME selectedEComStoreID;
        ApplicationState applicationState;

        public RestrictedSkusForm(GlobalConstants.STORENAME sTORENAME)
        {
            InitializeComponent();

            commonRepository = new CommonRepository();
            restrictedSkusRepository = new RestrictedSkusRepository();

            applicationState = ApplicationState.GetState;
            selectedEComStoreID = sTORENAME;

            fillDropDownLists();
            RefreshMainGrid();
        }

        private void fillDropDownLists()
        {
            try
            {
                clientApis = commonRepository.GetClientApis();

                foreach (ClientApi item in clientApis)
                {
                    ddlClientAPIs.Items.Add(item.ApiDescription);
                }

                ddlClientAPIs.SelectedIndex = 0;
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RefreshMainGrid()
        {
            String apiType;

            try
            {
                if (ddlClientAPIs.SelectedItem != null)
                {
                    apiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;

                    restrictedSkusList = restrictedSkusRepository.GetByClientAPI(apiType).Where(m => m.EcomStoreId == (int)selectedEComStoreID).ToList();

                    this.dgvRBGrid.DataSource = applicationState.LinqToDataTable<RestrictedSku>(restrictedSkusList);

                    this.dgvRBGrid.Columns["Id"].Visible = false;
                    this.dgvRBGrid.Columns["AddDate"].Visible = false;
                    this.dgvRBGrid.Columns["ApiType"].Visible = false;
                    this.dgvRBGrid.Columns["EcomStoreId"].Visible = false;
                    this.dgvRBGrid.Columns["EcomStore"].Visible = false;

                    this.dgvRBGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            RestrictedSkusRepository restrictedSkusRepository = new ();
            RestrictedSku restrictedSku = new RestrictedSku();
            string sku;

            try
            {

                sku = txtSKU.Text;

                if (sku == string.Empty)
                {
                    MessageBox.Show("Please provide all values to proceed");

                    return;
                }
                else
                {
                    restrictedSku.Sku = sku;
                    restrictedSku.AddDate = DateTime.Now;
                    restrictedSku.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                    restrictedSku.EcomStoreId = (int)selectedEComStoreID;

                    restrictedSkusRepository.Insert(restrictedSku);

                    restrictedSkusRepository.Save();

                    RefreshMainGrid();

                    txtSKU.Text = String.Empty;

                    txtSKU.Focus();
                }

            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvRBGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.Row!.Index];
            RestrictedSku restrictedSku = new(); 
            RestrictedSkusRepository restrictedSkusRepository = new();

            try
            {
                restrictedSkusRepository.Delete(Convert.ToInt32(dataGridViewRow.Cells["Id"].Value));

                restrictedSkusRepository.Save();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvRBGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.RowIndex];
            RestrictedSku restrictedSku = new RestrictedSku();
            RestrictedSkusRepository restrictedSkusRepository = new();

            try
            {
                restrictedSku.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedSku.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                restrictedSku.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                restrictedSku.AddDate = DateTime.Now;
                restrictedSku.EcomStoreId = (int)selectedEComStoreID;

                restrictedSkusRepository.Update(restrictedSku);

                restrictedSkusRepository.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                applicationState.LogErrorToFile(ex);
            }
        }

        private void ddlClientAPIs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshMainGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                applicationState.LogErrorToFile(ex);
            }
        }
    }
}
