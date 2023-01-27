using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;

namespace ShopifyInventorySync
{
    public partial class RestrictedBrandsForm : Form
    {
        List<ClientApi> clientApis = new();
        List<RestrictedBrand> restrictedBrandsList = new List<RestrictedBrand>();
        CommonRepository commonRepository;
        IRestrictedBrandsRepository restrictedBrandsRepository;
        public GlobalConstants.STORENAME selectedEComStoreID;
        ApplicationState applicationState;

        public RestrictedBrandsForm(GlobalConstants.STORENAME sTORENAME)
        {
            InitializeComponent();

            commonRepository = new CommonRepository();
            restrictedBrandsRepository = new RestrictedBrandsRepository();

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

        private void btnAdd_Click(object sender, EventArgs e)
        {
            RestrictedBrandsRepository restrictedBrandsContext = new RestrictedBrandsRepository();
            RestrictedBrand restrictedBrand = new();
            string brandName;

            try
            {

                brandName = txtBrandName.Text;

                if (brandName == string.Empty)
                {
                    MessageBox.Show("Please provide all values to proceed");

                    return;
                }
                else
                {
                    if(ddlClientAPIs.SelectedItem != null)
                    {
                        restrictedBrand.BrandName = brandName;
                        restrictedBrand.AddDate = DateTime.Now;
                        restrictedBrand.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                        restrictedBrand.EcomStoreId = (int)selectedEComStoreID;

                        restrictedBrandsContext.Insert(restrictedBrand);

                        restrictedBrandsContext.Save();

                        RefreshMainGrid();

                        txtBrandName.Text = String.Empty;

                        txtBrandName.Focus();
                    }
                    else
                    {
                        MessageBox.Show("Invalid client type selected");
                    }
                }

            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
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

                    restrictedBrandsList = restrictedBrandsRepository.GetByClientAPI(apiType).Where(m => m.EcomStoreId == (int)selectedEComStoreID).ToList();

                    this.dgvRBGrid.DataSource = applicationState.LinqToDataTable<RestrictedBrand>(restrictedBrandsList);

                    this.dgvRBGrid.Columns["Id"].Visible = false;
                    this.dgvRBGrid.Columns["AddDate"].Visible = false;
                    this.dgvRBGrid.Columns["ApiType"].Visible = false;
                    this.dgvRBGrid.Columns["EcomStoreId"].Visible = false;
                    this.dgvRBGrid.Columns["EcomStore"].Visible = false;

                    this.dgvRBGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                else
                {
                    MessageBox.Show("Invalid client type selected");
                }
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
            RestrictedBrand restrictedBrand = new RestrictedBrand();
            RestrictedBrandsRepository restrictedBrandsContext = new RestrictedBrandsRepository();

            try
            {
                if (ddlClientAPIs.SelectedItem != null)
                {
                    restrictedBrand.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                    restrictedBrand.BrandName = Convert.ToString(dataGridViewRow.Cells["BrandName"].Value);
                    restrictedBrand.ApiType = clientApis.Where(m => m.ApiDescription == ddlClientAPIs.SelectedItem.ToString()).FirstOrDefault()!.ApiType!;
                    restrictedBrand.AddDate = DateTime.Now;
                    restrictedBrand.EcomStoreId = (int)selectedEComStoreID;

                    restrictedBrandsContext.Update(restrictedBrand);

                    restrictedBrandsContext.Save();
                }
                else
                {
                    MessageBox.Show("Invalid client type selected");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                applicationState.LogErrorToFile(ex);
            }
        }

        private void dgvRBGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.Row!.Index];
            RestrictedBrandsRepository restrictedBrandsContext = new RestrictedBrandsRepository();

            try
            {
                restrictedBrandsContext.Delete(Convert.ToInt32(dataGridViewRow.Cells["Id"].Value));

                restrictedBrandsContext.Save();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void ddlClientAPIs_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshMainGrid();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
