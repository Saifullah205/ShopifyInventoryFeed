using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic
{
    internal sealed class ApplicationState
    {
        private static readonly object threadSafelock = new object ();  
        private static ApplicationState? instance = null;
        private IApplicationSettingsRepository applicationSettingsRepository;

        public List<MarkUpPrice> shopifyMarkUpPrice = new ();
        public List<ShopifyFixedPrice> shopifyFixedPricesList = new();
        public string processingMessages;

        public static ApplicationState GetState
        {
            get
            {
                if (instance == null)
                {
                    lock (threadSafelock)
                        {
                            if (instance == null)
                            {
                                instance = new ApplicationState();

                                instance.RefreshApplicationSettings();
                                instance.RefreshShopifyMarkUPPricesList();
                            }
                        }
                }

                return instance;
            }
        }

        public void RefreshApplicationSettings()
        {
            List<ApplicationSetting> applicationSettings = new ();

            try
            {
                applicationSettings.Clear();

                applicationSettingsRepository = new ApplicationSettingsRepository();

                applicationSettings = applicationSettingsRepository.GetAll().ToList<ApplicationSetting>();

                foreach (ApplicationSetting item in applicationSettings)
                {
                    SetGlobalConstants(item);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void RefreshShopifyMarkUPPricesList()
        {
            IMarkUpPriceRepository markUpPriceRepository = new MarkUpPriceRepository();

            try
            {
                shopifyMarkUpPrice.Clear();

                shopifyMarkUpPrice = markUpPriceRepository.GetAll().ToList<MarkUpPrice>();

            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshFixedPricesList()
        {
            IFixedPriceRespsitory fixedPriceRespsitory = new FixedPriceRespsitory();

            try
            {
                shopifyFixedPricesList.Clear();

                shopifyFixedPricesList = fixedPriceRespsitory.GetAll().ToList<ShopifyFixedPrice>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        public void LogErrorToFile(Exception ex)
        {
            WriteToErrorLog(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace);
        }

        public void WriteToErrorLog(String text)
        {
            try
            {
                string fileName = "ExceptionLog-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                lock (threadSafelock)
                {
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
            }
            catch (Exception)
            {
            }
        }

        public void SetGlobalConstants(ApplicationSetting applicationSetting)
        {
            string tag;
            string tagValue;

            try
            {
                tag = applicationSetting.Tag!;
                tagValue = applicationSetting.TagValue!;

                switch (tag.ToUpper())
                {
                    case "SHOPIFYSKUPREFIX":
                        GlobalConstants.shopifySKUPrefix = tagValue;
                        break;
                    case "MINIMUMQUANTITY":
                        GlobalConstants.minimumQuantity = tagValue;
                        break;
                    case "MARKOUTOFSTOCK":
                        GlobalConstants.markOutOfStock = tagValue;
                        break;
                    case "LOCATIONID":
                        GlobalConstants.locationId = tagValue;
                        break;
                    case "SHOPIFYACCESSKEY":
                        GlobalConstants.shopifyAccessKey = tagValue;
                        break;
                    case "SHOPIFYBASEURL":
                        GlobalConstants.shopifyBaseURL = tagValue;
                        break;
                    case "FRAGRANCEXSKUPREFIX":
                        GlobalConstants.fragranceXSKUPrefix = tagValue;
                        break;
                    case "APIACCESSID":
                        GlobalConstants.apiAccessId = tagValue;
                        break;
                    case "APIACCESSKEY":
                        GlobalConstants.apiAccessKey = tagValue;
                        break;
                    case "GRANT_TYPE":
                        GlobalConstants.grant_type = tagValue;
                        break;
                    case "FRAGRANCEXURL":
                        GlobalConstants.fragrancexURL = tagValue;
                        break;
                    case "FRAGRANCENETURL":
                        GlobalConstants.fragranceNetURL = tagValue;
                        break;
                    case "FRAGRANCENETUSERNAME":
                        GlobalConstants.fragranceNetUserName = tagValue;
                        break;
                    case "FRAGRANCENETPASSWORD":
                        GlobalConstants.fragranceNetPassword = tagValue;
                        break;
                    case "FRAGRANCENETSKUPREFIX":
                        GlobalConstants.fragranceNetSKUPrefix = tagValue;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable LinqToDataTable<T>(IEnumerable<T> items)
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

        public FileInfo ShowBrowseFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return new FileInfo(openFileDialog.FileName);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }

        public decimal CalculateShopifyMarkupPrice(decimal actualPrice)
        {
            decimal markedupPrice = actualPrice;

            foreach (MarkUpPrice markUpPriceItem in shopifyMarkUpPrice)
            {
                if (markUpPriceItem.MinPrice <= Math.Ceiling(actualPrice) && markUpPriceItem.MaxPrice >= Math.Ceiling(actualPrice))
                {
                    markedupPrice = Math.Round(actualPrice + ((markUpPriceItem.MarkupPercentage * actualPrice) / 100), 2);

                    break;
                }
            }

            return markedupPrice;
        }

        public void AddMessageToLogs(string message)
        {
            processingMessages += message + Environment.NewLine;
        }

        public void ClearLogMessages()
        {
            processingMessages = String.Empty;
        }
    }
}
