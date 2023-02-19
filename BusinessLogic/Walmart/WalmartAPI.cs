using Azure;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ShopifyInventorySync.BusinessLogic.Walmart
{
    public class WalmartAPI
    {
        ApplicationState applicationState;

        public WalmartAPI()
        {
            applicationState = ApplicationState.GetState;
        }

        public string GetWalmartToken()
        {
            WalmartTokenModel walmartToken = new();
            string url = GlobalConstants.walmartURL + "/token";

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Post);
                RestResponse response;

                request.AddHeader("WM_QOS.CORRELATION_ID", GlobalConstants.wmQosCorrelationId);
                request.AddHeader("WM_SVC.NAME", GlobalConstants.wmSvcName);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Authorization", GlobalConstants.walmartAuthorization);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("grant_type", "client_credentials");

                response = client.Execute(request);

                walmartToken = JsonConvert.DeserializeObject<WalmartTokenModel>(response.Content!)!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return walmartToken.access_token;
        }

        public string PostProductsToWalmart(string payLoadData, GlobalConstants.WALMARTFEEDTYPE wALMARTFEEDTYPE)
        {
            string walmartFeedType = string.Empty;
            string url = string.Empty;
            string resultResponse = string.Empty;

            try
            {
                if (wALMARTFEEDTYPE == GlobalConstants.WALMARTFEEDTYPE.MP_ITEM)
                {
                    walmartFeedType = "?feedType=MP_ITEM";
                }
                else if (wALMARTFEEDTYPE == GlobalConstants.WALMARTFEEDTYPE.MP_INVENTORY)
                {
                    walmartFeedType = "?feedType=MP_INVENTORY";
                }

                url = GlobalConstants.walmartURL + "/feeds" + walmartFeedType;

                RestClient client = new();
                RestRequest request = new(url, method: Method.Post);
                RestResponse response;

                request.AddHeader("WM_SEC.ACCESS_TOKEN", GetWalmartToken());
                request.AddHeader("WM_CONSUMER.CHANNEL.TYPE", GlobalConstants.wmConsumerChannelType);
                request.AddHeader("WM_QOS.CORRELATION_ID", GlobalConstants.wmQosCorrelationId);
                request.AddHeader("WM_SVC.NAME", GlobalConstants.wmSvcName);
                request.AddHeader("Authorization", GlobalConstants.walmartAuthorization);
                request.AddHeader("Accept", "application/json");
                request.AddHeader("Content-Type", "text/plain");

                request.AddParameter("text/plain", payLoadData, ParameterType.RequestBody);

                response = client.Execute(request);

                resultResponse = response.Content!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }

        public string GetWalmartFeedResponse(string feedId, bool includeDetails)
        {
            string url = GlobalConstants.walmartURL + "/feeds/" + feedId + (includeDetails ? "?includeDetails=true" : string.Empty);
            string resultResponse = string.Empty;

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Get);
                RestResponse response;

                request.AddHeader("WM_SEC.ACCESS_TOKEN", GetWalmartToken());
                //request.AddHeader("WM_CONSUMER.CHANNEL.TYPE", GlobalConstants.wmConsumerChannelType);
                request.AddHeader("WM_QOS.CORRELATION_ID", GlobalConstants.wmQosCorrelationId);
                request.AddHeader("WM_SVC.NAME", GlobalConstants.wmSvcName);
                request.AddHeader("Authorization", GlobalConstants.walmartAuthorization);
                request.AddHeader("Accept", "application/json");

                response = client.Execute(request);

                resultResponse = response.Content!;
            }
            catch (Exception ex)
            {
                applicationState.LogErrorToFile(ex);
            }

            return resultResponse;
        }

        public string RetireWalmartFeed(string sku)
        {
            string url = GlobalConstants.walmartURL + "/items/" + sku;
            string resultResponse = string.Empty;

            try
            {
                RestClient client = new();
                RestRequest request = new(url, method: Method.Delete);
                RestResponse response;

                request.AddHeader("WM_SEC.ACCESS_TOKEN", GetWalmartToken());
                request.AddHeader("WM_CONSUMER.CHANNEL.TYPE", GlobalConstants.wmConsumerChannelType);
                request.AddHeader("WM_QOS.CORRELATION_ID", GlobalConstants.wmQosCorrelationId);
                request.AddHeader("WM_SVC.NAME", GlobalConstants.wmSvcName);
                request.AddHeader("Authorization", GlobalConstants.walmartAuthorization);
                request.AddHeader("Accept", "application/json");

                response = client.Execute(request);

                resultResponse = response.Content!;

                applicationState.LogInfoToFile(url);
                applicationState.LogInfoToFile(resultResponse);
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
    }
}
