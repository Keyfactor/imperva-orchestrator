using System;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Orchestrators.Common.Enums;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleOrchestratorExtension
{
    public class Management : IManagementJobExtension
    {
        //Necessary to implement IManagementJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        //Job Entry Point
        public JobResult ProcessJob(ManagementJobConfiguration config)
        {
            //METHOD ARGUMENTS...
            //config - contains context information passed from KF Command to this job run:
            //
            // config.Server.Username, config.Server.Password - credentials for orchestrated server - use to authenticate to certificate store server.
            //
            // config.ServerUsername, config.ServerPassword - credentials for orchestrated server - use to authenticate to certificate store server.
            // config.CertificateStoreDetails.ClientMachine - server name or IP address of orchestrated server
            // config.CertificateStoreDetails.StorePath - location path of certificate store on orchestrated server
            // config.CertificateStoreDetails.StorePassword - if the certificate store has a password, it would be passed here
            // config.CertificateStoreDetails.Properties - JSON string containing custom store properties for this specific store type
            //
            // config.JobCertificate.EntryContents - Base64 encoded string representation (PKCS12 if private key is included, DER if not) of the certificate to add for Management-Add jobs.
            // config.JobCertificate.Alias - optional string value of certificate alias (used in java keystores and some other store types)
            // config.OpeerationType - enumeration representing function with job type.  Used only with Management jobs where this value determines whether the Management job is a CREATE/ADD/REMOVE job.
            // config.Overwrite - Boolean value telling the Orchestrator Extension whether to overwrite an existing certificate in a store.  How you determine whether a certificate is "the same" as the one provided is AnyAgent implementation dependent
            // config.JobCertificate.PrivateKeyPassword - For a Management Add job, if the certificate being added includes the private key (therefore, a pfx is passed in config.JobCertificate.EntryContents), this will be the password for the pfx.


            //NLog Logging to c:\CMS\Logs\CMS_Agent_Log.txt
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