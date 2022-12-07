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

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    class APIProcessor
    {
        private string ApiURL { get; set; }
        private string ApiID { get; set; }
        private string ApiKey { get; set; }


        #region Public Methods
        public async Task Initialize(string apiURL, string apiID, string apiKey)
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            ApiURL = apiURL;
            ApiID = apiID;
            ApiKey = apiKey;

            logger.MethodExit(LogLevel.Debug);
        }

        public async Task<List<Site>> GetSites()
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            string rtnMessage = string.Empty;

            string RESOURCE = $"/crypto/v1/keys";
            RestRequest request = new RestRequest(RESOURCE, Method.Get);

            logger.MethodExit(LogLevel.Debug);

            return JsonConvert.DeserializeObject<List<SecurityObject>>(await SubmitRequest(request, BearerToken)).Where(p => p.Type.ToUpper() == "CERTIFICATE").ToList();
        }
        #endregion

        private async Task<string> SubmitRequest(RestRequest request, string auth)
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
            request.AddHeader("Authorization", auth);

            try
            {
                response = await client.ExecuteAsync(request);
            }
            catch (Exception ex)
            {
                string exceptionMessage = FortanixException.FlattenExceptionMessages(ex, $"Error processing {request.Resource}");
                logger.LogError(exceptionMessage);
                throw new FortanixException(exceptionMessage);
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK &&
                response.StatusCode != System.Net.HttpStatusCode.Accepted &&
                response.StatusCode != System.Net.HttpStatusCode.Created &&
                response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                string errorMessage;

                try
                {
                    APIError error = JsonConvert.DeserializeObject<APIError>(response.Content);
                    errorMessage = $"{error.Message}";
                }
                catch (JsonReaderException ex)
                {
                    errorMessage = response.Content;
                }

                string exceptionMessage = $"Error processing {request.Resource}: {errorMessage}";
                logger.LogError(exceptionMessage);
                throw new FortanixException(exceptionMessage);
            }

            logger.LogTrace($"API Result: {response.Content}");
            logger.MethodExit(LogLevel.Debug);

            return response.Content;
        }
    }
}
