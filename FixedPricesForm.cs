using ShopifyInventorySync.Models;
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
        List<ShopifyFixedPrice> shopifyFixedPricesList = new List<ShopifyFixedPrice>();
        public FixedPricesForm()
        {
            InitializeComponent();

            RefreshMainGrid();

            this.dgvFixedPrice.Columns["Id"].Visible = false;
            this.dgvFixedPrice.Columns["AddDate"].Visible = false;

            this.dgvFixedPrice.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ShopifyDbContext shopifyDBContext = new();
            ShopifyFixedPrice shopifyFixedPrice = new();

            try
            {
                if (string.IsNullOrEmpty(txtfixedPrice.Text) || string.IsNullOrEmpty(txtsku.Text))
                {
                    MessageBox.Show("Please provide all valid values to proceed");

                    return;
                }
                else
                {
                    shopifyFixedPrice.FixedPrice = txtfixedPrice.Text;
                    shopifyFixedPrice.Sku = txtsku.Text;
                    shopifyFixedPrice.AddDate = DateTime.Now;

                    shopifyDBContext.ShopifyFixedPrices.Add(shopifyFixedPrice);
                    shopifyDBContext.SaveChanges();
                    RefreshMainGrid();

                    txtfixedPrice.Text = String.Empty;
                    txtsku.Text = String.Empty;

                    txtsku.Focus();
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
                shopifyFixedPricesList = shopifyDBContext.ShopifyFixedPrices.ToList<ShopifyFixedPrice>();

                this.dgvFixedPrice.DataSource = SharedFunctions.LinqToDataTable<ShopifyFixedPrice>(shopifyFixedPricesList);
            }
            catch (Exception ex)
            {
                SharedFunctions.logErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void dgvFixedPrice_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvFixedPrice.Rows[e.RowIndex];
            ShopifyFixedPrice shopifyFixedPrice = new();
            ShopifyDbContext shopifyDBContext = new ShopifyDbContext();

            try
            {
                shopifyFixedPrice.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                shopifyFixedPrice.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                shopifyFixedPrice.FixedPrice = Convert.ToString(dataGridViewRow.Cells["FixedPrice"].Value);
                shopifyFixedPrice.AddDate = DateTime.Now;

                shopifyDBContext.ShopifyFixedPrices.Update(shopifyFixedPrice);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

                SharedFunctions.logErrorToFile(ex);
            }
        }

        private void dgvFixedPrice_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = dgvFixedPrice.Rows[e.Row!.Index];
            ShopifyFixedPrice shopifyFixedPrice = new();
            ShopifyDbContext shopifyDBContext = new();

            try
            {
                shopifyFixedPrice.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                shopifyFixedPrice.Sku = Convert.ToString(dataGridViewRow.Cells["Sku"].Value);
                shopifyFixedPrice.FixedPrice = Convert.ToString(dataGridViewRow.Cells["FixedPrice"].Value);
                shopifyFixedPrice.AddDate = DateTime.Now;

                shopifyDBContext.ShopifyFixedPrices.Remove(shopifyFixedPrice);

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
