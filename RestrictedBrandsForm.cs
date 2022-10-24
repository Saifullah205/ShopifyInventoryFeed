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
    public partial class RestrictedBrandsForm : Form
    {
        List<RestrictedBrand> restrictedBrandsList = new List<RestrictedBrand>();

        public RestrictedBrandsForm()
        {
            InitializeComponent();

            RefreshMainGrid();

            this.dgvRBGrid.Columns["Id"].Visible = false;
            this.dgvRBGrid.Columns["AddDate"].Visible = false;

            this.dgvRBGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ShopifyDbContext shopifyDBContext = new();
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
                    restrictedBrand.BrandName = brandName;
                    restrictedBrand.AddDate = DateTime.Now;

                    shopifyDBContext.RestrictedBrands.Add(restrictedBrand);

                    shopifyDBContext.SaveChanges();

                    RefreshMainGrid();

                    txtBrandName.Text = String.Empty;

                    txtBrandName.Focus();
                }

            }
            catch (Exception ex)
            {
                SharedFunctions.logErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshMainGrid()
        {
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                restrictedBrandsList = shopifyDBContext.RestrictedBrands.ToList<RestrictedBrand>();

                this.dgvRBGrid.DataSource = SharedFunctions.LinqToDataTable<RestrictedBrand>(restrictedBrandsList);
            }
            catch (Exception ex)
            {
                SharedFunctions.logErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvRBGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.RowIndex];
            RestrictedBrand restrictedBrand = new RestrictedBrand();
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                restrictedBrand.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedBrand.BrandName = Convert.ToString(dataGridViewRow.Cells["BrandName"].Value);
                restrictedBrand.AddDate = DateTime.Now;

                shopifyDBContext.RestrictedBrands.Update(restrictedBrand);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                SharedFunctions.logErrorToFile(ex);
            }
        }

        private void dgvRBGrid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvRBGrid.Rows[e.Row!.Index];
            RestrictedBrand restrictedBrand = new();
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                restrictedBrand.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                restrictedBrand.BrandName = Convert.ToString(dataGridViewRow.Cells["BrandName"].Value);
                restrictedBrand.AddDate = DateTime.Now;

                shopifyDBContext.RestrictedBrands.Remove(restrictedBrand);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                SharedFunctions.logErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }
    }
}
