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
using ShopifyInventorySync.Models;

namespace ShopifyInventorySync
{
    public partial class ApplicationSettingsForm : Form
    {
        List<ApplicationSetting> applicationSettingsList = new List<ApplicationSetting>();

        public ApplicationSettingsForm()
        {
            InitializeComponent();

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

                this.DGVApplicationSettings.DataSource = this.LinqToDataTable<ApplicationSetting>(applicationSettingsList);
            }
            catch (Exception ex)
            {
                logErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void DGVApplicationSettings_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow dataGridViewRow = DGVApplicationSettings.Rows[e.RowIndex];
            ApplicationSetting applicationSetting = new ();
            EFDbContext shopifyDBContext = new ();

            try
            {
                applicationSetting.Id = Convert.ToInt32(dataGridViewRow.Cells["Id"].Value);
                applicationSetting.Tag = Convert.ToString(dataGridViewRow.Cells["Tag"].Value);
                applicationSetting.TagValue = Convert.ToString(dataGridViewRow.Cells["TagValue"].Value);
                applicationSetting.AddDate = DateTime.Now;

                shopifyDBContext.ApplicationSettings.Update(applicationSetting);

                shopifyDBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private DataTable LinqToDataTable<T>(IEnumerable<T> items)
        {
            //Createa DataTable with the Name of the Class i.e. Customer class.
            DataTable dt = new DataTable(typeof(T).Name);

            //Read all the properties of the Class i.e. Customer class.
            PropertyInfo[] propInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            //Loop through each property of the Class i.e. Customer class.
            foreach (PropertyInfo propInfo in propInfos)
            {
                //Add Columns in DataTable based on Property Name and Type.
                dt.Columns.Add(new DataColumn(propInfo.Name, Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType));
            }

            //Loop through the items if the Collection.
            foreach (T item in items)
            {
                //Add a new Row to DataTable.
                DataRow dr = dt.Rows.Add();

                //Loop through each property of the Class i.e. Customer class.
                foreach (PropertyInfo propInfo in propInfos)
                {
                    //Add value Column to the DataRow.
                    dr[propInfo.Name] = propInfo.GetValue(item, null);
                }
            }

            return dt;
        }

        private void logErrorToFile(Exception ex)
        {
            WriteToErrorLog(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace);
        }

        private void WriteToErrorLog(String text)
        {
            string fileName = "ExceptionLog-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "ErrorLog")))
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "ErrorLog"));

                if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName)))
                {
                    File.Create(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName));
                }
            }

            File.AppendAllText(Path.Combine(Environment.CurrentDirectory, "ErrorLog", fileName), text + Environment.NewLine);
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
