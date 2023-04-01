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
using Microsoft.EntityFrameworkCore;
using ShopifyInventorySync.BusinessLogic;
using ShopifyInventorySync.Models;

namespace ShopifyInventorySync
{
    public partial class ApplicationSettingsForm : Form
    {
        List<ApplicationSetting> applicationSettingsList = new List<ApplicationSetting>();
        ApplicationState applicationState;

        public ApplicationSettingsForm()
        {
            InitializeComponent();

            applicationState = ApplicationState.GetState;
            RefreshApplicationSettingsGrid();

            this.DGVApplicationSettings.Columns["Id"].Visible = false;
            this.DGVApplicationSettings.Columns["AddDate"].Visible = false;
            this.DGVApplicationSettings.Columns["Tag"].ReadOnly = true;

            this.DGVApplicationSettings.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void RefreshApplicationSettingsGrid()
        {
            EFDbContext shopifyDBContext = new();

            try
            {
                applicationSettingsList = shopifyDBContext.ApplicationSettings.ToList<ApplicationSetting>();

                this.DGVApplicationSettings.DataSource = applicationState.LinqToDataTable<ApplicationSetting>(applicationSettingsList);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void DGVApplicationSettings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = DGVApplicationSettings.Rows[e.RowIndex];
            ApplicationSetting applicationSetting = new ();
            EFDbContext efDbContext = new ();

            try
            {
                applicationSetting.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                applicationSetting.Tag = Convert.ToString(dataGridViewRow.Cells["Tag"].Value);
                applicationSetting.TagValue = Convert.ToString(dataGridViewRow.Cells["TagValue"].Value);
                applicationSetting.AddDate = DateTime.Now;

                efDbContext.ApplicationSettings.Update(applicationSetting);

                efDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnResetData_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure to delete all products data?", "Confirm", MessageBoxButtons.YesNo);
            EFDbContext shopifyDBContext = new ();

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    shopifyDBContext.Database.ExecuteSqlRaw("SELECT * INTO dbo.bkp_ShopifyInventoryData_" + DateTime.Now.ToString("MMddyyyyHHmmss") + " FROM dbo.ShopifyInventoryData");
                    shopifyDBContext.Database.ExecuteSqlRaw("TRUNCATE TABLE dbo.ShopifyInventoryData");
                }
                catch (Exception)
                {

                    throw;
                }         
            }
        }
    }
}
