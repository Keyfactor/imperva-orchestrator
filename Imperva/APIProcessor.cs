using Keyfactor.Extensions.Orchestrator.Imperva.Models;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keyfactor.Extensions.Orchestrator.Imperva.Model;
using Newtonsoft.Json.Linq;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    class APIProcessor
    {
        private string ApiURL { get; set; }
        private string AccountID { get; set; }
        private string ApiID { get; set; }
        private string ApiKey { get; set; }


        #region Public Methods
        public async Task Initialize(string apiURL, string accountID, string apiID, string apiKey)
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            AccountID = accountID;
            ApiURL = apiURL;
            ApiID = apiID;
            ApiKey = apiKey;

            logger.MethodExit(LogLevel.Debug);
        }

        public async Task<List<Site>> GetSites()
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            List<Site> sites = new List<Site>();

            int page = 0;

            string RESOURCE = $"/api/prov/v1/sites/list?account_id={AccountID}&page_size=50&page_num={page.ToString()}";
            RestRequest request = new RestRequest(RESOURCE, Method.Post);

            do
            {
                JObject json = JObject.Parse(await SubmitRequest(request));
                List<Site> pageOfSites = JsonConvert.DeserializeObject<List<Site>>(json.SelectToken("sites").ToString());
                if (pageOfSites.Count == 0)
                    break;
                else
                    page++;
                sites.AddRange(pageOfSites);
            } while (1 == 1);


            logger.MethodExit(LogLevel.Debug);

            return sites;
        }
        #endregion

        private async Task<string> SubmitRequest(RestRequest request)
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);
            logger.LogTrace($"Request Resource: {request.Resource}");
            logger.LogTrace($"Request Method: {request.Method.ToString()}");
            if (request.Method != Method.Get)
            {
                StringBuilder body = new StringBuilder("Request Body: ");
                foreach (Parameter parameter in request.Parameters)
                {
                    body.Append($"{parameter.Name}={parameter.Value}");
                }
                logger.LogTrace(body.ToString());
            }

            RestResponse response;

            RestClient client = new RestClient(ApiURL);
            request.AddHeader("x-API-Id", ApiID);
            request.AddHeader("x-API-Key", ApiKey);

            try
            {
                response = await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                string exceptionMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error processing {request.Resource}");
                logger.LogError(exceptionMessage);
                throw new ImpervaException(exceptionMessage);
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Accepted &&
                response.StatusCode != System.Net.HttpStatusCode.Created &&
                response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                string errorMessage = response.Content + " " + response.ErrorMessage;
                string exceptionMessage = $"Error processing {request.Resource}: {errorMessage}";

                logger.LogError(exceptionMessage);
                throw new ImpervaException(exceptionMessage);
            }

            ValidateResponse(response.Content);

            logger.LogTrace($"API Result: {response.Content}");
            logger.MethodExit(LogLevel.Debug);

            return response.Content;
        }
    }
}
