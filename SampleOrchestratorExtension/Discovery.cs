using System;
using System.Collections.Generic;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;

using Microsoft.Extensions.Logging;

namespace Keyfactor.Extensions.Orchestrator.SampleOrchestratorExtension
{
    // The Discovery class implementes IAgentJobExtension and is meant to find all certificate stores based on the information passed when creating the job in KF Command 
    public class Discovery : IDiscoveryJobExtension
    {
        //Necessary to implement IDiscoveryJobExtension but not used.  Leave as empty string.
        public string ExtensionName => "";

        //Job Entry Point
        public JobResult ProcessJob(DiscoveryJobConfiguration config, SubmitDiscoveryUpdate submitDiscovery)
        {
            //METHOD ARGUMENTS...
            //config - contains context information passed from KF Command to this job run:
            //
            // config.ServerUsername, config.ServerPassword - credentials for orchestrated server - use to authenticate to certificate store server.
            // config.ClientMachine - server name or IP address of orchestrated server
            //
            // config.JobProperties["dirs"] - Directories to search
            // config.JobProperties["extensions"] - Extensions to search
            // config.JobProperties["ignoreddirs"] - Directories to ignore
            // config.JobProperties["patterns"] - File name patterns to match


            //NLog Logging to c:\CMS\Logs\CMS_Agent_Log.txt
            ILogger logger = LogHandler.GetClassLogger(this.GetType());
            logger.LogDebug($"Begin Discovery...");

            //Instantiate collection of certificate store locations to pass back
            List<string> locations = new List<string>();

            try
            {
                //Code logic to:
                // 1) Connect to the orchestrated server if necessary (config.CertificateStoreDetails.ClientMachine)
                // 2) Custom logic to search for valid certificate stores based on passed in:
                //      a) Directories to search
                //      b) Extensions
                //      c) Directories to ignore
                //      d) File name patterns to match
                // 3) Place found and validated store locations (path and file name) in "locations" collection instantiated above
            }
            catch (Exception ex)
            {
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = "Custom message you want to show to show up as the error message in Job History in KF Command" };
            }

            try
            {
                //Sends store locations back to KF command where they can be approved or rejected
                submitDiscovery.Invoke(locations);
                //Status: 2=Success, 3=Warning, 4=Error
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                // NOTE: if the cause of the submitInventory.Invoke exception is a communication issue between the Orchestrator server and the Command server, the job status returned here
                //  may not be reflected in Keyfactor Command.
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = "Custom message you want to show to show up as the error message in Job History in KF Command" };
            }
        }
    }
}