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

        #region Public Methods
        public void AddCertificate(int siteID, string certificate, string password)
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            CertificateRequestPUT certificateRequest = new CertificateRequestPUT() { Certificate = certificate, Password = password };
            string RESOURCE = $"/api/prov/v2/sites/{siteID.ToString()}/customCertificate";
            RestRequest request = new RestRequest(RESOURCE, Method.Put);
            request.AddParameter("application/json", JsonConvert.SerializeObject(certificateRequest), ParameterType.RequestBody);

            JObject json = SubmitRequest(request);

            logger.MethodExit(LogLevel.Debug);

            return;
        }

        public void RemoveCertificate(int siteID)
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            string RESOURCE = $"/api/prov/v2/sites/{siteID.ToString()}/customCertificate?auth_type=RSA";
            RestRequest request = new RestRequest(RESOURCE, Method.Delete);

            JObject json = SubmitRequest(request);

            logger.MethodExit(LogLevel.Debug);

            return;
        }

        public List<Site> GetSites()
        {
            ILogger logger = LogHandler.GetClassLogger<APIProcessor>();
            logger.MethodEntry(LogLevel.Debug);

            List<Site> sites = new List<Site>();

            int page = 0;

            do
            {
                string RESOURCE = $"/api/prov/v1/sites/list?account_id={AccountID}&page_size=50&page_num={page.ToString()}";
                RestRequest request = new RestRequest(RESOURCE, Method.Post);

                JObject json = SubmitRequest(request);
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

        public X509Certificate2 GetServerCertificateAsync(string url)
        {
            if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                url = "https://" + url;

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

            try
            {
                httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).GetAwaiter().GetResult();
            }
            catch (HttpRequestException) { }

            return certificate;
        }
        #endregion

        private JObject SubmitRequest(RestRequest request)
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
                response = client.Execute(request);
                logger.MethodExit(LogLevel.Debug);
            }
            catch (Exception ex)
            {
                string exceptionMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error processing {request.Resource}");
                logger.LogError(exceptionMessage);
                throw ex;
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

            string err = ValidateResponse(json);
            if (!string.IsNullOrEmpty(err))
                throw new ImpervaException($"Error processing {request.Resource}: {err}");

            logger.LogTrace($"API Result: {response.Content}");

            return json;
        }

        private string ValidateResponse(JObject json)
        {
            int returnCode = Convert.ToInt32(JsonConvert.DeserializeObject<string>(json.SelectToken("res").ToString()));

            return returnCode == 0 ? String.Empty : $"Return code = {returnCode.ToString()}, Error = {json.SelectToken("res_message").ToString()}, Debug Info = {json.SelectToken("debug_info").ToString()}";
        }
    }
}
