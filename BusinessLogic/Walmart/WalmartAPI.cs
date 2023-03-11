using Azure;
using Newtonsoft.Json;
using RestSharp;
using ShopifyInventorySync.Models;
using ShopifyInventorySync.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static ShopifyInventorySync.BusinessLogic.GlobalConstants;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    public class WalmartAPI
    {
        ApplicationState applicationState;

        public WalmartAPI()
        {
            applicationState = ApplicationState.GetState;

            GetWalmartToken();
        }

        #region Data Context Methods
        public void ProcessProductToWalmart(string walmartProductsTextData, WALMARTFEEDTYPE wALMARTFEEDTYPE)
        {
            try
            {
                string response = PostProductsToWalmart(walmartProductsTextData, wALMARTFEEDTYPE);
                string feedType = string.Empty;

                if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_INVENTORY)
                {
                    feedType = WALMARTFEEDTYPEPOST.INVENTORYFEED.ToString();
                }
                else if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_ITEM)
                {
                    feedType = WALMARTFEEDTYPEPOST.SETUPITEM.ToString();
                }
                else if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_SHIPPINGMAP)
                {
                    feedType = WALMARTFEEDTYPEPOST.MAPSHIPPINGTEMPLATE.ToString();
                }

                SaveFeedResponse(response, feedType);
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        private void SaveFeedResponse(string response, string feedType)
        {
            WalmartFeedObject walmartFeed = new();
            WalmartFeedResponse walmartFeedResponse = new();
            WalmartFeedResponseRepository WalmartFeedRepository = new();

            try
            {
                walmartFeed = JsonConvert.DeserializeObject<WalmartFeedObject>(response)!;

                walmartFeedResponse.FeedId = walmartFeed.feedId;
                walmartFeedResponse.FeedType = feedType;
                walmartFeedResponse.EcomStoreId = (int)STORENAME.WALMART;

                WalmartFeedRepository.Insert(walmartFeedResponse);

                WalmartFeedRepository.Save();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }
        #endregion

        #region API Methods
        public void GetWalmartToken()
        {
            WalmartTokenModel walmartToken = new();
            string url = WALMARTURL + "/token";

            try
            {
                if (string.IsNullOrEmpty(WALMRTTOKEN))
                {
                    RestClient client = new();
                    RestRequest request = new(url, method: Method.Post);
                    RestResponse response;

                    request.AddHeader("WM_QOS.CORRELATION_ID", WMQOSCORRELATIONID);
                    request.AddHeader("WM_SVC.NAME", WMSVCNAME);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Authorization", WALMARTAUTHORIZATION);
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddParameter("grant_type", "client_credentials");

                    response = client.Execute(request);

                    walmartToken = JsonConvert.DeserializeObject<WalmartTokenModel>(response.Content!)!;

                    WALMRTTOKEN = walmartToken.access_token;
                }                
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }
        }

        public string PostProductsToWalmart(string payLoadData, WALMARTFEEDTYPE wALMARTFEEDTYPE)
        {
            string walmartFeedType = string.Empty;
            string url = string.Empty;
            string resultResponse = string.Empty;
            int retryCount = 0;

            try
            {
                while(retryCount < 3)
                {
                    if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_ITEM)
                    {
                        url = WALMARTURL + "/feeds?feedType=MP_ITEM";
                    }
                    else if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_INVENTORY)
                    {
                        url = WALMARTURL + "/feeds?feedType=MP_INVENTORY";
                    }
                    else if (wALMARTFEEDTYPE == WALMARTFEEDTYPE.MP_SHIPPINGMAP)
                    {
                        url = WALMARTURL + "/feeds?feedType=SKU_TEMPLATE_MAP";
                    }

                    RestClient client = new();
                    RestRequest request = new(url, method: Method.Post);
                    RestResponse response;

                    request.AddHeader("WM_SEC.ACCESS_TOKEN", WALMRTTOKEN);
                    request.AddHeader("WM_CONSUMER.CHANNEL.TYPE", WMCONSUMERCHANNELTYPE);
                    request.AddHeader("WM_QOS.CORRELATION_ID", WMQOSCORRELATIONID);
                    request.AddHeader("WM_SVC.NAME", WMSVCNAME);
                    request.AddHeader("Authorization", WALMARTAUTHORIZATION);
                    request.AddHeader("Accept", "application/json");
                    request.AddHeader("Content-Type", "text/plain");

                    request.AddParameter("text/plain", payLoadData, ParameterType.RequestBody);

                    response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        resultResponse = response.Content!;

                        retryCount = 0;

                        break;
                    }
                    else if(response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        WALMRTTOKEN = string.Empty;

                        GetWalmartToken();

                        retryCount++;
                    }
                    else
                    {
                        applicationState.LogInfoToFile(response.Content!.ToString());

                        break;
                    }
                }                
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }

        public string GetWalmartFeedResponse(string feedId, bool includeDetails)
        {
            string url = WALMARTURL + "/feeds/" + feedId + (includeDetails ? "?includeDetails=true" : string.Empty);
            string resultResponse = string.Empty;
            int retryCount = 0;

            try
            {
                while (retryCount < 3)
                {
                    RestClient client = new();
                    RestRequest request = new(url, method: Method.Get);
                    RestResponse response;

                    request.AddHeader("WM_SEC.ACCESS_TOKEN", WALMRTTOKEN);
                    request.AddHeader("WM_QOS.CORRELATION_ID", WMQOSCORRELATIONID);
                    request.AddHeader("WM_SVC.NAME", WMSVCNAME);
                    request.AddHeader("Authorization", WALMARTAUTHORIZATION);
                    request.AddHeader("Accept", "application/json");

                    response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        resultResponse = response.Content!;

                        retryCount = 0;

                        break;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        WALMRTTOKEN = string.Empty;

                        GetWalmartToken();

                        retryCount++;
                    }
                    else
                    {
                        applicationState.LogInfoToFile(response.Content!.ToString());

                        break;
                    }
                }                    
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }

        public string RetireWalmartFeed(string sku)
        {
            string url = WALMARTURL + "/items/" + sku;
            string resultResponse = string.Empty;
            int retryCount = 0;

            try
            {
                while (retryCount < 3)
                {
                    RestClient client = new();
                    RestRequest request = new(url, method: Method.Delete);
                    RestResponse response;

                    request.AddHeader("WM_SEC.ACCESS_TOKEN", WALMRTTOKEN);
                    request.AddHeader("WM_CONSUMER.CHANNEL.TYPE", WMCONSUMERCHANNELTYPE);
                    request.AddHeader("WM_QOS.CORRELATION_ID", WMQOSCORRELATIONID);
                    request.AddHeader("WM_SVC.NAME", WMSVCNAME);
                    request.AddHeader("Authorization", WALMARTAUTHORIZATION);
                    request.AddHeader("Accept", "application/json");

                    response = client.Execute(request);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        resultResponse = response.Content!;

                        retryCount = 0;

                        break;
                    }
                    else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        WALMRTTOKEN = string.Empty;

                        GetWalmartToken();

                        retryCount++;
                    }
                    else
                    {
                        applicationState.LogInfoToFile(response.Content!.ToString());

                        break;
                    }

                    applicationState.LogInfoToFile(url);
                    applicationState.LogInfoToFile(resultResponse);
                }
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }

        public string FormatWalmartFeedResponse(string feedResponse)
        {
            string url = "https://jsonformatter.curiousconcept.com/?data=" + HttpUtility.UrlEncode(feedResponse) + "&process=true";
            string resultResponse = string.Empty;
            Process myProcess = new Process();

            try
            {
                myProcess.StartInfo.UseShellExecute = true;
                myProcess.StartInfo.FileName = url;
                myProcess.Start();
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }
        #endregion
    }
}
