// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Keyfactor.Logging;

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

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
        public APIProcessor(string apiURL, string accountID, string apiID, string apiKey)
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

            do
            {
                try
                {
                    RestRequest request = new RestRequest(RESOURCE, Method.Post);

                    JObject json = await SubmitRequest(request);
                    List<Site> pageOfSites = JsonConvert.DeserializeObject<List<Site>>(json.SelectToken("sites").ToString());
                    if (pageOfSites.Count == 0)
                        break;
                    else
                        page++;

                    sites.AddRange(pageOfSites);
                }
                catch (Exception ex)
                {
                    throw new ImpervaException("Error calling or parsing response for /api/prov/v1/sites/list", ex);
                }
            } while (1 == 1);


            logger.MethodExit(LogLevel.Debug);

            return sites;
        }
        public async Task<X509Certificate2> GetServerCertificateAsync(string url)
        {
            X509Certificate2 certificate = null;
            var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, cert, __, ___) =>
                {
                    certificate = new X509Certificate2(cert.GetRawCertData());
                    return true;
                }
            };

            var httpClient = new HttpClient(httpClientHandler);
            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            return certificate ?? new X509Certificate2();
        }
        #endregion

        private async Task<JObject> SubmitRequest(RestRequest request)
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

            JObject json = JObject.Parse(response.Content);

            ValidateResponse(json);

            logger.LogTrace($"API Result: {response.Content}");
            logger.MethodExit(LogLevel.Debug);

            return json;
        }

        private void ValidateResponse(JObject json)
        {
            int returnCode = Convert.ToInt32(JsonConvert.DeserializeObject<string>(json.SelectToken("res").ToString()));

            if (returnCode != 0)
                throw new ImpervaException($"Return code = {returnCode.ToString()}, Error = {json.SelectToken("res_message").ToString()}, Debug Info = {json.SelectToken("debug_info").ToString()}");
        }
    }
}
