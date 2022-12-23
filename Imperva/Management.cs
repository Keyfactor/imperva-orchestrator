// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Linq;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Extensions.Orchestrator.Imperva.Model;

using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Keyfactor.Orchestrators.Extensions.Interfaces;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    public class Management : BaseJob, IManagementJobExtension
    {
        //Necessary to implement IManagementJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        public IPAMSecretResolver _resolver;

        public Management(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
        }

        //Job Entry Point
        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Server: {config.CertificateStoreDetails.ClientMachine}");
            logger.LogDebug($"Store Path: {config.CertificateStoreDetails.StorePath}");

            try
            {
                string storePassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Store Password", config.CertificateStoreDetails.StorePassword);

                SetAPIProperties(config.CertificateStoreDetails.ClientMachine, config.CertificateStoreDetails.StorePath, storePassword);

                if (config.OperationType != CertStoreOperationType.Add && config.OperationType != CertStoreOperationType.Remove)
                    return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}" };

                APIProcessor api = new APIProcessor(ApiURL, AccountID, ApiID, ApiKey);

                List<Site> sites = api.GetSites();

                Site site = sites.FirstOrDefault(p => p.Domain == config.JobCertificate.Alias);
                if (site == null)
                    throw new ImpervaException($"Site {config.JobCertificate.Alias} is not currently being managed by Imperva.  No certificate can be bound to this site.");

                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:

                        if (!config.Overwrite && api.GetServerCertificateAsync(site.Domain) != null)
                            return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Warning, JobHistoryId = config.JobHistoryId, FailureMessage = $"Overwrite is set to false but there is a certificate that already is bound to {config.JobCertificate.Alias}.  Please set overwrite to true and reschedule the job to replace this certificate." };

                        api.AddCertificate(site.SiteID, config.JobCertificate.Contents, config.JobCertificate.PrivateKeyPassword);

                        break;
                    case CertStoreOperationType.Remove:
                        api.RemoveCertificate(site.SiteID);
                        break;
                }
            }
            catch (Exception ex)
            {
                string errMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error processing Imperva {config.Capability} job, URL {ApiURL}, AccountID {AccountID}, Site {config.JobCertificate.Alias}. ");
                logger.LogError(errMessage);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = errMessage };
            }

            return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }
    }
}