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
using ShopifyInventorySync.Models;

namespace ShopifyInventorySync
{
    public partial class RestrictedSkusForm : Form
    {
        List<RestrictedSku> restrictedSkusList = new List<RestrictedSku>();
        public RestrictedSkusForm()
        {
            InitializeComponent();

            RefreshMainGrid();

            this.dgvRBGrid.Columns["Id"].Visible = false;
            this.dgvRBGrid.Columns["AddDate"].Visible = false;

            this.dgvRBGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void RefreshMainGrid()
        {
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                restrictedSkusList = shopifyDBContext.RestrictedSkus.ToList<RestrictedSku>();

                this.dgvRBGrid.DataSource = SharedFunctions.LinqToDataTable<RestrictedSku>(restrictedSkusList);
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();
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

                    shopifyDBContext.RestrictedSkus.Add(restrictedSku);

                    shopifyDBContext.SaveChanges();

                    RefreshMainGrid();

                    txtSKU.Text = String.Empty;

                    txtSKU.Focus();
                }

            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvRBGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.Row!.Index];
            RestrictedSku restrictedSku = new();
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                restrictedSku.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedSku.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                restrictedSku.AddDate = DateTime.Now;

                shopifyDBContext.RestrictedSkus.Remove(restrictedSku);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvRBGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.RowIndex];
            RestrictedSku restrictedSku = new RestrictedSku();
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                restrictedSku.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedSku.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                restrictedSku.AddDate = DateTime.Now;

                shopifyDBContext.RestrictedSkus.Update(restrictedSku);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                SharedFunctions.LogErrorToFile(ex);
            }
        }
    }
}
