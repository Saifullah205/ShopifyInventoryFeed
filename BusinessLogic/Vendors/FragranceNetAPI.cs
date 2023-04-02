using CsvHelper;
using System.Globalization;
using System.Text;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Vendors
{
    public class FragranceNetAPI
    {
        ApplicationState applicationState;

        public FragranceNetAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        private async Task<string> GetFragranceNetAPIData()
        {
            HttpClient client;
            HttpResponseMessage response;
            string responseBody = string.Empty;
            string authKey;

            try
            {
                authKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(FRAGRANCENETUSERNAME + ":" + FRAGRANCENETPASSWORD));

                client = new();
                client.DefaultRequestHeaders.Add("Authorization", "Basic " + authKey);
                response = await client.GetAsync(FRAGRANCENETURL);
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception)
            {
                throw;
            }

            return responseBody;
        }

        public async Task<FragranceNetProductsList> FetchDataFromAPI()
        {
            string fragranceXJsonData = string.Empty;
            Task<string> apiResponseData;
            string fileTextData = string.Empty;
            string csvTextData = string.Empty;
            FragranceNetProductsList fragranceNetProducts = new();
            byte[] csvBytes;
            List<FragranceXProduct> loadedFragranceXProducts = new();

            try
            {
                apiResponseData = GetFragranceNetAPIData();

                fileTextData = await apiResponseData;              

                if (!string.IsNullOrEmpty(fileTextData))
                {
                    fileTextData = fileTextData.Replace("\"", "");
                    fileTextData = "\"" + fileTextData;
                    fileTextData = fileTextData.Replace("\t", "\",\"");
                    csvTextData = fileTextData.Replace("\n", "\"\n\"");

                    csvTextData = csvTextData.Substring(0, csvTextData.Length - 1);

                    csvBytes = Encoding.UTF8.GetBytes(csvTextData);

                    using (var reader = new StreamReader(new MemoryStream(csvBytes)))
                    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    {
                        fragranceNetProducts.products = csv.GetRecords<FragranceNetProduct>().Where(m => m.designer != "" && m.upc != "").ToList();
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
