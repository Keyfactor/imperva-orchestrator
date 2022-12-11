// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Extensions.Orchestrator.Imperva.Model;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    public class Inventory : BaseJob, IInventoryJobExtension
    {
        public string ExtensionName => "";

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Inventory...");

            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();

            try
            {
                SetAPIProperties(config.CertificateStoreDetails.ClientMachine, config.CertificateStoreDetails.StorePath, config.CertificateStoreDetails.StorePassword);

                APIProcessor api = new APIProcessor(ApiURL, AccountID, ApiID, ApiKey);
                List<Site> sites = api.GetSites().Result;

                foreach(Site site in sites)
                {
                    X509Certificate2 certificate = api.GetServerCertificateAsync(site.Domain).Result;
                    if (string.IsNullOrEmpty(certificate.Thumbprint))
                        continue;
                    inventoryItems.Add(new CurrentInventoryItem()
                    {
                        Alias = site.SiteID.ToString(),
                        Certificates = new string[] { Convert.ToBase64String(certificate.Export(X509ContentType.Cert)) },
                        ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                        UseChainLevel = false,
                        PrivateKeyEntry = true,
                        Parameters = new Dictionary<string, object>() { { "Domain", site.Domain } }
                    });
                }
            }
            catch (Exception ex)
            {
                string errMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error processing Imperva Inventory job, URL {ApiURL}, AccountID {AccountID}. ");
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = errMessage };
            }

            try
            {
                submitInventory.Invoke(inventoryItems);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                string errMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error returning cerificates for Imperva Inventory job, URL {ApiURL}, AccountID {AccountID}");
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = errMessage };
            }
        }
    }
}