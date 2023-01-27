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
    public partial class FixedPricesForm : Form
    {
        List<ClientApi> clientApis = new();
        List<FixedPrice> shopifyFixedPricesList = new List<FixedPrice>();
        CommonRepository commonRepository;
        IMarkUpPriceRepository markUpPriceRepository;
        GlobalConstants.STORENAME selectedEComStoreID;
        ApplicationState applicationState;

        public FixedPricesForm(GlobalConstants.STORENAME sTORENAME)
        {
            InitializeComponent();

            commonRepository = new CommonRepository();
            markUpPriceRepository = new MarkUpPriceRepository();

            applicationState = ApplicationState.GetState;
            selectedEComStoreID = sTORENAME;

            RefreshMainGrid();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            EFDbContext efDBContext = new();
            FixedPrice shopifyFixedPrice = new();

            try
            {
                if (string.IsNullOrEmpty(txtfixedPrice.Text) || string.IsNullOrEmpty(txtsku.Text))
                {
                    MessageBox.Show("Please provide all valid values to proceed");

                    return;
                }
                else
                {
                    shopifyFixedPrice.FixPrice = txtfixedPrice.Text;
                    shopifyFixedPrice.Sku = txtsku.Text;
                    shopifyFixedPrice.DateCreated = DateTime.Now;
                    shopifyFixedPrice.ApiType = "ALL";
                    shopifyFixedPrice.EcomStoreId = (int)selectedEComStoreID;

                    efDBContext.FixedPrices.Add(shopifyFixedPrice);
                    efDBContext.SaveChanges();

                    RefreshMainGrid();

                    txtfixedPrice.Text = String.Empty;
                    txtsku.Text = String.Empty;

                    txtsku.Focus();
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
            FixedPriceRespsitory fixedPriceRespsitory = new();

            try
            {
                shopifyFixedPricesList = fixedPriceRespsitory.GetAll().Where(m => m.EcomStoreId == (int)selectedEComStoreID).ToList<FixedPrice>();

                this.dgvFixedPrice.DataSource = applicationState.LinqToDataTable<FixedPrice>(shopifyFixedPricesList);

                this.dgvFixedPrice.Columns["Id"].Visible = false;
                this.dgvFixedPrice.Columns["DateCreated"].Visible = false;
                this.dgvFixedPrice.Columns["ApiType"].Visible = false;
                this.dgvFixedPrice.Columns["EcomStoreId"].Visible = false;
                this.dgvFixedPrice.Columns["EcomStore"].Visible = false;

                this.dgvFixedPrice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvFixedPrice_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvFixedPrice.Rows[e.RowIndex];
            FixedPrice shopifyFixedPrice = new();
            FixedPriceRespsitory fixedPriceRespsitory = new();

            try
            {
                shopifyFixedPrice.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                shopifyFixedPrice.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                shopifyFixedPrice.FixPrice = Convert.ToString(dataGridViewRow.Cells["FixPrice"].Value);
                shopifyFixedPrice.DateCreated = DateTime.Now;
                shopifyFixedPrice.ApiType = "ALL";
                shopifyFixedPrice.EcomStoreId = (int)selectedEComStoreID;

                fixedPriceRespsitory.Update(shopifyFixedPrice);

                fixedPriceRespsitory.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                applicationState.LogErrorToFile(ex);
            }
        }

        private void dgvFixedPrice_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvFixedPrice.Rows[e.Row!.Index];
            FixedPrice shopifyFixedPrice = new();
            FixedPriceRespsitory fixedPriceRespsitory = new();

            try
            {
                fixedPriceRespsitory.Delete(Convert.ToInt32(dataGridViewRow.Cells["Id"].Value));

                fixedPriceRespsitory.Save();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
    }
}
