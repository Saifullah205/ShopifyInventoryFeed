using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;


namespace ShopifyInventorySync
{
    public partial class RestrictedTermsForm : Form
    {
        CommonRepository commonRepository;
        IRestrictedTermsRepository restrictedTermsRepository;
        List<ClientApi> clientApis = new();
        List<RestrictedTerm> restrictedTermList = new List<RestrictedTerm>();
        public STORENAME selectedEComStoreID;
        ApplicationState applicationState;

        public RestrictedTermsForm(STORENAME sTORENAME)
        {
            InitializeComponent();

            commonRepository = new CommonRepository();
            restrictedTermsRepository = new RestrictedTermsRepository();

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

                    restrictedTermList = restrictedTermsRepository.GetByClientAPI(apiType).Where(m => m.EcomStoreId == (int)selectedEComStoreID).ToList();

                    this.dgvRBGrid.DataSource = applicationState.LinqToDataTable<RestrictedTerm>(restrictedTermList);

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

        private void dgvRBGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.Row!.Index];
            RestrictedTermsRepository restrictedTermRepository = new();

            try
            {
                restrictedTermRepository.Delete(Convert.ToInt32(dataGridViewRow.Cells["Id"].Value));

                restrictedTermRepository.Save();
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
            RestrictedTerm restrictedTerm = new RestrictedTerm();
            RestrictedTermsRepository restrictedTermRepository = new();

            try
            {
                restrictedTerm.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedTerm.Term = Convert.ToString(dataGridViewRow.Cells["Term"].Value);
                restrictedTerm.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                restrictedTerm.AddDate = DateTime.Now;
                restrictedTerm.EcomStoreId = (int)selectedEComStoreID;

                restrictedTermRepository.Update(restrictedTerm);

                restrictedTermRepository.Save();
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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            RestrictedTermsRepository restrictedTermsRepository = new();
            RestrictedTerm restrictedTerm = new RestrictedTerm();
            string term;

            try
            {

                term = txtSKU.Text;

                if (term == string.Empty)
                {
                    MessageBox.Show("Please provide all values to proceed");

                    return;
                }
                else
                {
                    restrictedTerm.Term = term;
                    restrictedTerm.AddDate = DateTime.Now;
                    restrictedTerm.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                    restrictedTerm.EcomStoreId = (int)selectedEComStoreID;

                    restrictedTermsRepository.Insert(restrictedTerm);

                    restrictedTermsRepository.Save();

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
    }
}
