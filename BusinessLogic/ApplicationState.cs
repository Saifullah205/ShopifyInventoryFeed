using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System.Data;
using System.Reflection;

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

                shopifyMarkUpPrice = markUpPriceRepository.GetAll().Where(m => m.EcomStoreId == (int)GlobalConstants.STORENAME.SHOPIFY).ToList<MarkUpPrice>();
                walmartMarkUpPrice = markUpPriceRepository.GetAll().Where(m => m.EcomStoreId == (int)GlobalConstants.STORENAME.WALMART).ToList<MarkUpPrice>();
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
            WriteToLogToFile(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + ex.Message + " : " + ex.StackTrace, GlobalConstants.LOGTYPE.ERROR);
        }

        public void LogInfoToFile(string message)
        {
            WriteToLogToFile(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + " : " + message, GlobalConstants.LOGTYPE.INFO);
        }

        public void WriteToLogToFile(String text, GlobalConstants.LOGTYPE lOGTYPE)
        {
            try
            {
                string fileType = lOGTYPE == GlobalConstants.LOGTYPE.ERROR ? "Error" : "Info";
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
                        GlobalConstants.tpsSKUPrefix = tagValue;
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
                    case "REQUIRESSHIPPING":
                        GlobalConstants.requiresShipping = tagValue.ToUpper() == "Y" ? true : false;
                        break;
                    case "WM_CONSUMER.CHANNEL.TYPE":
                        GlobalConstants.wmConsumerChannelType = tagValue;
                        break;
                    case "WM_QOS.CORRELATION_ID":
                        GlobalConstants.wmQosCorrelationId = tagValue;
                        break;
                    case "WM_SVC.NAME":
                        GlobalConstants.wmSvcName = tagValue;
                        break;
                    case "WALMARTAUTHORIZATION":
                        GlobalConstants.walmartAuthorization = tagValue;
                        break;
                    case "WALMARTURL":
                        GlobalConstants.walmartURL = tagValue;
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

        public decimal GetMarkedUpPrice(string sku, string cost, GlobalConstants.STORENAME storeName)
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

        public decimal CalculateMarkupPrice(decimal actualPrice, GlobalConstants.STORENAME storeName)
        {
            decimal markedupPrice = actualPrice;
            List<MarkUpPrice> MarkUpPriceList = new();

            try
            {
                if (storeName == GlobalConstants.STORENAME.SHOPIFY)
                {
                    MarkUpPriceList = shopifyMarkUpPrice;
                }
                else if (storeName == GlobalConstants.STORENAME.WALMART)
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
