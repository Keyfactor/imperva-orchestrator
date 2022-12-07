// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    public class Management : IManagementJobExtension
    {
        //Necessary to implement IManagementJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        //Job Entry Point
        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Management...");

            try
            {
                //Management jobs, unlike Discovery, Inventory, and Reenrollment jobs can have 3 different purposes:
                switch (config.OperationType)
                {
                    case CertStoreOperationType.Add:
                        //OperationType == Add - Add a certificate to the certificate store passed in the config object
                        //Code logic to:
                        // 1) Connect to the orchestrated server (config.CertificateStoreDetails.ClientMachine) containing the certificate store
                        // 2) Custom logic to add certificate to certificate store (config.CertificateStoreDetails.StorePath) possibly using alias as an identifier if applicable (config.JobCertificate.Alias).  Use alias and overwrite flag (config.Overwrite)
                        //     to determine if job should overwrite an existing certificate in the store, for example a renewal.
                        break;
                    case CertStoreOperationType.Remove:
                        //OperationType == Remove - Delete a certificate from the certificate store passed in the config object
                        //Code logic to:
                        // 1) Connect to the orchestrated server (config.CertificateStoreDetails.ClientMachine) containing the certificate store
                        // 2) Custom logic to remove the certificate in a certificate store (config.CertificateStoreDetails.StorePath), possibly using alias (config.JobCertificate.Alias) or certificate thumbprint to identify the certificate (implementation dependent)
                        break;
                    case CertStoreOperationType.Create:
                        //OperationType == Create - Create an empty certificate store in the provided location
                        //Code logic to:
                        // 1) Connect to the orchestrated server (config.CertificateStoreDetails.ClientMachine) where the certificate store (config.CertificateStoreDetails.StorePath) will be located
                        // 2) Custom logic to first check if the store already exists and add it if not.  If it already exists, implementation dependent as to how to handle - error, warning, success
                        break;
                    default:
                        //Invalid OperationType.  Return error.  Should never happen though
                        return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = $"Site {config.CertificateStoreDetails.StorePath} on server {config.CertificateStoreDetails.ClientMachine}: Unsupported operation: {config.OperationType.ToString()}" };
                }
            }
            catch (Exception ex)
            {
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = "Custom message you want to show to show up as the error message in Job History in KF Command" };
            }

            //Status: 2=Success, 3=Warning, 4=Error
            return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
        }
    }
}