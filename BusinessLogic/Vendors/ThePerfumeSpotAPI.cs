using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopifyInventorySync.BusinessLogic.Vendors
{
    public class ThePerfumeSpotAPI
    {
        ApplicationState applicationState;

        public ThePerfumeSpotAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        public string FetchDataFromAPI()
        {
            string responseData = string.Empty;
            FileInfo fileInfo;

            try
            {
                fileInfo = applicationState.ShowBrowseFileDialog();

                if (fileInfo != null)
                {
                    responseData = File.ReadAllText(fileInfo.FullName);
                }

                if (string.IsNullOrEmpty(responseData))
                {
                    return string.Empty;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return responseData;
        }

        public ThePerfumeSpotProductsList GetDataFromSource()
        {
            string fileTextData;
            ThePerfumeSpotProductsList fragranceNetProducts = new();
            byte[] csvBytes;

            try
            {
                fileTextData = FetchDataFromAPI();

                if (!string.IsNullOrEmpty(fileTextData))
                {
                    csvBytes = Encoding.UTF8.GetBytes(fileTextData);

                    using (var reader = new StreamReader(new MemoryStream(csvBytes)))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        fragranceNetProducts.products = csv.GetRecords<ThePerfumeSpotProduct>().ToList();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceNetProducts;
        }
    }
}
