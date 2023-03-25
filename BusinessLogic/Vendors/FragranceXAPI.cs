using Newtonsoft.Json;
using RestSharp;
using ShopifyInventorySync.BusinessLogic.Shopify;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Vendors
{
    public class FragranceXAPI
    {
        ApplicationState applicationState;

        public FragranceXAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        private async Task<string> GetFragranceXAPIToken()
        {
            string token = string.Empty;
            string url = FRAGRANCEXURL + "/token";
            FragranceXToken fragranceXToken = new();

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("grant_type", GRANT_TYPE);
                request.AddParameter("apiAccessId", APIACCESSID);
                request.AddParameter("apiAccessKey", APIACCESSKEY);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    fragranceXToken = JsonConvert.DeserializeObject<FragranceXToken>(response.Content!)!;

                    token = fragranceXToken.access_token;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show("Unable to connect FragranceX API");
            }

            return token;
        }

        private async Task<string> GetFragranceXAPIData(string token)
        {
            string productsData = string.Empty;
            string url = FRAGRANCEXURL + "/product/list/";

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url, Method.Get);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);

                RestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    productsData = response.Content!;
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return productsData;
        }

        public async Task<List<FragranceXProduct>> FetchDataFromAPI()
        {
            Task<string> fragranceXJsonData;
            string fragranceXResponseData;
            List<FragranceXProduct> loadedFragranceXProducts = new();
            string token = string.Empty;

            try
            {
                token = await GetFragranceXAPIToken();

                if (!string.IsNullOrEmpty(token))
                {
                    fragranceXJsonData = GetFragranceXAPIData(token);

                    fragranceXResponseData = await fragranceXJsonData;

                    loadedFragranceXProducts = JsonConvert.DeserializeObject<List<FragranceXProduct>>(fragranceXResponseData!)!;
                }
                else
                {
                    MessageBox.Show("FragranceX API not accessible");
                }

            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);

                MessageBox.Show(ex.Message);
            }

            return loadedFragranceXProducts;
        }

        public async Task<FragranceXProductsList> GetDataFromSource()
        {
            FragranceXProductsList fragranceXProductsList = new();
            List<FragranceXProduct> products;

            try
            {
                products = await FetchDataFromAPI();

                fragranceXProductsList.products = products.Where(m => m.Upc != "").ToList();
            }
            catch (Exception)
            {
                throw;
            }

            return fragranceXProductsList;
        }
    }
}
