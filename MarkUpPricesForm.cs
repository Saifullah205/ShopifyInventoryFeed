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
using ShopifyInventorySync.Repositories;

namespace ShopifyInventorySync
{
    public partial class MarkUpPricesForm : Form
    {
        List<ClientApi> clientApis = new();
        List<MarkUpPrice> markUpPricesList = new List<MarkUpPrice>();
        CommonRepository commonRepository;
        IMarkUpPriceRepository markUpPriceRepository;

        public MarkUpPricesForm()
        {
            InitializeComponent();

            commonRepository = new CommonRepository();
            markUpPriceRepository = new MarkUpPriceRepository();

            try
            {
                RefreshMarkUpPricesGrid();
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            MarkUpPriceRepository markUpPriceRepositoryContext = new();
            MarkUpPrice markUpPrice = new MarkUpPrice();
            List<MarkUpPrice> markUpPricesList = new List<MarkUpPrice>();
            bool priceChecksPassed = false;
            decimal minPrice;
            decimal maxPrice;
            decimal markupPriceAmount;

            try
            {

                minPrice = Convert.ToDecimal(txtMinPrice.Text);
                maxPrice = Convert.ToDecimal(txtMaxPrice.Text);
                markupPriceAmount = Convert.ToDecimal(txtMarkupAmount.Text);

                if (minPrice == 0 || maxPrice == 0 || markupPriceAmount == 0)
                {
                    MessageBox.Show("Please provide all values to proceed");

                    return;
                }
                else if (minPrice >= maxPrice)
                {
                    MessageBox.Show("Max price cannot be smaller/equal to min price range");

                    return;
                }
                else
                {
                    markUpPricesList = markUpPriceRepositoryContext.GetAll().ToList<MarkUpPrice>();

                    foreach (MarkUpPrice markUpPriceI in markUpPricesList)
                    {
                        if((minPrice < markUpPriceI.MinPrice && maxPrice < markUpPriceI.MinPrice) || (minPrice > markUpPriceI.MaxPrice && maxPrice > markUpPriceI.MaxPrice))
                        {
                            priceChecksPassed = true;
                        }
                        else
                        {
                            priceChecksPassed = false;

                            MessageBox.Show("Price range cannot be overlapped.");

                            return;
                        }
                    }

                    if (priceChecksPassed)
                    {
                        markUpPrice.MinPrice = minPrice;
                        markUpPrice.MaxPrice = maxPrice;
                        markUpPrice.MarkupPercentage = markupPriceAmount;
                        markUpPrice.AddDate = DateTime.Now;
                        markUpPrice.ApiType = "ALL";

                        markUpPriceRepositoryContext.Insert(markUpPrice);

                        markUpPriceRepositoryContext.Save();

                        RefreshMarkUpPricesGrid();

                        txtMinPrice.Text = String.Empty;
                        txtMaxPrice.Text = String.Empty;
                        txtMarkupAmount.Text = String.Empty;

                        txtMinPrice.Focus();
                    }
                }

            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshMarkUpPricesGrid()
        {
            try
            {
                markUpPricesList = markUpPriceRepository.GetAll().OrderBy(m => m.MinPrice).ToList<MarkUpPrice>();

                this.DGVMarkUpPrices.DataSource = SharedFunctions.LinqToDataTable<MarkUpPrice>(markUpPricesList);

                this.DGVMarkUpPrices.Columns["Id"].Visible = false;
                this.DGVMarkUpPrices.Columns["AddDate"].Visible = false;
                this.DGVMarkUpPrices.Columns["ApiType"].Visible = false;

                this.DGVMarkUpPrices.Columns["MinPrice"].ReadOnly = true;
                this.DGVMarkUpPrices.Columns["MaxPrice"].ReadOnly = true;

                this.DGVMarkUpPrices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void DGVMarkUpPrices_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = DGVMarkUpPrices.Rows[e.RowIndex];
            MarkUpPrice markUpPrice = new ();
            MarkUpPriceRepository markUpPriceRepositoryContext = new();

            try
            {
                markUpPrice.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                markUpPrice.MinPrice = Convert.ToDecimal(dataGridViewRow.Cells["MinPrice"].Value);
                markUpPrice.MaxPrice = Convert.ToDecimal(dataGridViewRow.Cells["MaxPrice"].Value);
                markUpPrice.MarkupPercentage = Convert.ToDecimal(dataGridViewRow.Cells["MarkupPercentage"].Value);
                markUpPrice.AddDate = DateTime.Now;
                markUpPrice.ApiType = "ALL";

                markUpPriceRepositoryContext.Update(markUpPrice);

                markUpPriceRepositoryContext.Save();
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void DGVMarkUpPrices_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dataGridViewRow = DGVMarkUpPrices.Rows[e.Row!.Index];
            MarkUpPrice markUpPrice = new();
            MarkUpPriceRepository markUpPriceRepositoryContext = new();

            try
            {
                markUpPrice.MinPrice = Convert.ToDecimal(dataGridViewRow.Cells["MinPrice"].Value);
                markUpPrice.MaxPrice = Convert.ToDecimal(dataGridViewRow.Cells["MaxPrice"].Value);
                markUpPrice.MarkupPercentage = Convert.ToDecimal(dataGridViewRow.Cells["MarkupPercentage"].Value);
                markUpPrice.AddDate = DateTime.Now;

                markUpPriceRepositoryContext.Delete(Convert.ToInt32(dataGridViewRow.Cells["Id"].Value));

                markUpPriceRepositoryContext.Save();
            }
            catch (Exception ex)
            {
                SharedFunctions.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void ddlClientAPIs_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
