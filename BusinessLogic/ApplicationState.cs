using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;
using System.Reflection;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic
{
    internal sealed class ApplicationState
    {
        private static readonly object threadSafelock = new object();
        private static ApplicationState? instance = null;
        private IApplicationSettingsRepository applicationSettingsRepository;

        public List<MarkUpPrice> shopifyMarkUpPrice = new();
        public List<MarkUpPrice> walmartMarkUpPrice = new();
        public List<FixedPrice> shopifyFixedPricesList = new();
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
                            instance.RefreshMarkUPPricesList();
                            instance.RefreshFixedPricesList();
                        }
                    }
                }

                return instance;
            }
        }

        public void RefreshApplicationSettings()
        {
            List<ApplicationSetting> applicationSettings = new();

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

        public void RefreshMarkUPPricesList()
        {
            IMarkUpPriceRepository markUpPriceRepository = new MarkUpPriceRepository();

            try
            {
                shopifyMarkUpPrice.Clear();
                walmartMarkUpPrice.Clear();

                shopifyMarkUpPrice = markUpPriceRepository.GetAll().Where(m => m.EcomStoreId == (int)STORENAME.SHOPIFY).ToList<MarkUpPrice>();
                walmartMarkUpPrice = markUpPriceRepository.GetAll().Where(m => m.EcomStoreId == (int)STORENAME.WALMART).ToList<MarkUpPrice>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        public void RefreshFixedPricesList()
        {
            IFixedPriceRespsitory fixedPriceRespsitory = new FixedPriceRespsitory();

            try
            {
                shopifyFixedPricesList.Clear();

                shopifyFixedPricesList = fixedPriceRespsitory.GetAll().ToList<FixedPrice>();
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }
        }

        public void LogErrorToFile(Exception ex)
        {
            WriteToLogToFile(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace, LOGTYPE.ERROR);
        }

        public void LogInfoToFile(string message)
        {
            WriteToLogToFile(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + message, LOGTYPE.INFO);
        }

        public void WriteToLogToFile(String text, LOGTYPE lOGTYPE)
        {
            try
            {
                string fileType = lOGTYPE == LOGTYPE.ERROR ? "Error" : "Info";
                string fileName = fileType + "Log-" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

                lock (threadSafelock)
                {
                    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, fileType + "Log")))
                    {
                        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, fileType + "Log"));

                        if (!File.Exists(Path.Combine(Environment.CurrentDirectory, fileType + "Log", fileName)))
                        {
                            File.Create(Path.Combine(Environment.CurrentDirectory, fileType + "Log", fileName));
                        }
                    }

                    File.AppendAllText(Path.Combine(Environment.CurrentDirectory, fileType + "Log", fileName), text + Environment.NewLine);
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
                        TPSSKUPREFIX = tagValue;
                        break;
                    case "MINIMUMQUANTITY":
                        MINIMUMQUANTITY = tagValue;
                        break;
                    case "MARKOUTOFSTOCK":
                        MARKOUTOFSTOCK = tagValue;
                        break;
                    case "LOCATIONID":
                        LOCATIONID = tagValue;
                        break;
                    case "SHOPIFYACCESSKEY":
                        SHOPIFYACCESSKEY = tagValue;
                        break;
                    case "SHOPIFYBASEURL":
                        SHOPIFYBASEURL = tagValue;
                        break;
                    case "FRAGRANCEXSKUPREFIX":
                        FRAGRANCEXSKUPREFIX = tagValue;
                        break;
                    case "APIACCESSID":
                        APIACCESSID = tagValue;
                        break;
                    case "APIACCESSKEY":
                        APIACCESSKEY = tagValue;
                        break;
                    case "GRANT_TYPE":
                        GRANT_TYPE = tagValue;
                        break;
                    case "FRAGRANCEXURL":
                        FRAGRANCEXURL = tagValue;
                        break;
                    case "FRAGRANCENETURL":
                        FRAGRANCENETURL = tagValue;
                        break;
                    case "FRAGRANCENETUSERNAME":
                        FRAGRANCENETUSERNAME = tagValue;
                        break;
                    case "FRAGRANCENETPASSWORD":
                        FRAGRANCENETPASSWORD = tagValue;
                        break;
                    case "FRAGRANCENETSKUPREFIX":
                        FRAGRANCENETSKUPREFIX = tagValue;
                        break;
                    case "REQUIRESSHIPPING":
                        REQUIRESSHIPPING = tagValue.ToUpper() == "Y" ? true : false;
                        break;
                    case "WM_CONSUMER.CHANNEL.TYPE":
                        WMCONSUMERCHANNELTYPE = tagValue;
                        break;
                    case "WM_QOS.CORRELATION_ID":
                        WMQOSCORRELATIONID = tagValue;
                        break;
                    case "WM_SVC.NAME":
                        WMSVCNAME = tagValue;
                        break;
                    case "WALMARTAUTHORIZATION":
                        WALMARTAUTHORIZATION = tagValue;
                        break;
                    case "WALMARTURL":
                        WALMARTURL = tagValue;
                        break;
                    case "SHIPPINGTEMPLATEID":
                        SHIPPINGTEMPLATEID = tagValue;
                        break;
                    case "FULFILLMENTCENTERID":
                        FULFILLMENTCENTERID = tagValue;
                        break;
                    case "WALMARTCHUNKSIZE":
                        WALMARTCHUNKSIZE = tagValue;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                throw;
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

        public decimal GetMarkedUpPrice(string sku, string cost, STORENAME storeName)
        {
            FixedPrice? ProductFixedPrice = new();
            bool isFixedPrice = false;
            decimal updatedCost = 0;

            try
            {
                ProductFixedPrice = shopifyFixedPricesList.Where(m => m.Sku == sku && m.EcomStoreId == (int)storeName).FirstOrDefault();

                if (ProductFixedPrice != null)
                {
                    try
                    {
                        if (Convert.ToDecimal(ProductFixedPrice!.FixPrice) > 0)
                        {
                            cost = ProductFixedPrice.FixPrice!;

                            isFixedPrice = true;
                        }
                    }
                    catch (Exception)
                    {
                        isFixedPrice = false;
                    }
                }

                if (isFixedPrice)
                {
                    updatedCost = Convert.ToDecimal(cost);
                }
                else
                {
                    updatedCost = CalculateMarkupPrice(Convert.ToDecimal(cost), storeName);
                }
            }
            catch (Exception ex)
            {
                LogErrorToFile(ex);
            }

            return updatedCost;
        }

        public decimal CalculateMarkupPrice(decimal actualPrice, STORENAME storeName)
        {
            decimal markedupPrice = actualPrice;
            List<MarkUpPrice> MarkUpPriceList = new();

            try
            {
                if (storeName == STORENAME.SHOPIFY)
                {
                    MarkUpPriceList = shopifyMarkUpPrice;
                }
                else if (storeName == STORENAME.WALMART)
                {
                    MarkUpPriceList = walmartMarkUpPrice;
                }

                if (MarkUpPriceList.Count > 0)
                {
                    foreach (MarkUpPrice markUpPriceItem in MarkUpPriceList)
                    {
                        if (markUpPriceItem.MinPrice <= Math.Ceiling(actualPrice) && markUpPriceItem.MaxPrice >= Math.Ceiling(actualPrice))
                        {
                            markedupPrice = Math.Round(actualPrice + ((markUpPriceItem.MarkupPercentage * actualPrice) / 100), 2);

                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
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
