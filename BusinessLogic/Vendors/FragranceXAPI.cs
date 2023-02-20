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

namespace ShopifyInventorySync.BusinessLogic.Vendors
{
    public class FragranceXAPI
    {
        ApplicationState applicationState;

        public FragranceXAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        private string GetFragranceXAPIToken()
        {
            string token = string.Empty;
            string url = GlobalConstants.fragrancexURL + "/token";
            FragranceXToken fragranceXToken = new();

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url, Method.Post);

                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("grant_type", GlobalConstants.grant_type);
                request.AddParameter("apiAccessId", GlobalConstants.apiAccessId);
                request.AddParameter("apiAccessKey", GlobalConstants.apiAccessKey);

                RestResponse response = client.Execute(request);

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

        private string GetFragranceXAPIData()
        {
            string productsData = string.Empty;
            string url = GlobalConstants.fragrancexURL + "/product/list/";

            try
            {
                RestClient client = new RestClient();
                var request = new RestRequest(url, Method.Get);

                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + GetFragranceXAPIToken());

                RestResponse response = client.Execute(request);

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

        public List<FragranceXProduct> FetchDataFromAPI()
        {
            string fragranceXJsonData = string.Empty;
            List<FragranceXProduct> loadedFragranceXProducts = new();
            string token = string.Empty;

            try
            {
                token = GetFragranceXAPIToken();

                if (!string.IsNullOrEmpty(token))
                {
                    //fragranceXJsonData = GetFragranceXAPIData(token);

                    fragranceXJsonData = File.ReadAllText(Environment.CurrentDirectory + "//FragranceX_Single.txt");

                    loadedFragranceXProducts = JsonConvert.DeserializeObject<List<FragranceXProduct>>(fragranceXJsonData!)!;
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
    }
}
