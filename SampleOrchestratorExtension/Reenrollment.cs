using System;
using System.Collections.Generic;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleOrchestratorExtension
{
    // The Reenrollment class implementes IAgentJobExtension and is meant to:
    //  1) Generate a new public/private keypair locally
    //  2) Generate a CSR from the keypair,
    //  3) Submit the CSR to KF Command to enroll the certificate and retrieve the certificate back
    //  4) Deploy the newly re-enrolled certificate to a certificate store
    public class Reenrollment : IReenrollmentJobExtension
    {
        //Necessary to implement IReenrollmentJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        //Job Entry Point
        public JobResult ProcessJob(ReenrollmentJobConfiguration config, SubmitReenrollmentCSR submitReenrollment)
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
            // config.JobProperties = Dictionary of custom parameters to use in building CSR and placing enrolled certiciate in a the proper certificate store

            //NLog Logging to c:\CMS\Logs\CMS_Agent_Log.txt
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Reenrollment...");

            try
            {
                //Code logic to:
                //  1) Generate a new public/private keypair locally from any config.JobProperties passed
                //  2) Generate a CSR from the keypair (PKCS10),
                //  3) Submit the CSR to KF Command to enroll the certificate using:
                //      string resp = (string)submitEnrollmentRequest.Invoke(Convert.ToBase64String(PKCS10_bytes);
                //      X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(resp));
                //  4) Deploy the newly re-enrolled certificate (cert in #3) to a certificate store
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