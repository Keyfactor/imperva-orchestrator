// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

using Keyfactor.Logging;
using Keyfactor.Orchestrators.Extensions;
using Keyfactor.Extensions.Orchestrator.Imperva.Model;
using Keyfactor.Orchestrators.Common.Enums;
using Keyfactor.Orchestrators.Extensions.Interfaces;
using Keyfactor.PKI;

using Microsoft.Extensions.Logging;
using Keyfactor.PKI.X509;
using System.Text;
using Keyfactor.PKI.CryptographicObjects.Formatters;

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    public class Inventory : BaseJob, IInventoryJobExtension
    {
        public string ExtensionName => "";

        public IPAMSecretResolver _resolver;

        private ILogger logger;

        public Inventory(IPAMSecretResolver resolver)
        {
            _resolver = resolver;
            logger = LogHandler.GetClassLogger(this.GetType());
        }

        public JobResult ProcessJob(InventoryJobConfiguration config, SubmitInventoryUpdate submitInventory)
        {
            logger.LogDebug($"Begin {config.Capability} for job id {config.JobId}...");
            logger.LogDebug($"Store Path: {config.CertificateStoreDetails.StorePath}");

            List<CurrentInventoryItem> inventoryItems = new List<CurrentInventoryItem>();
            bool oneOrMoreErrors = false;

            try
            {
                string storePassword = PAMUtilities.ResolvePAMField(_resolver, logger, "Store Password", config.CertificateStoreDetails.StorePassword);

                SetAPIProperties(config.CertificateStoreDetails.StorePath, storePassword);

                APIProcessor api = new APIProcessor(AccountID, ApiID, ApiKey);
                List<Site> sites = api.GetSites();

                foreach(Site site in sites)
                {
                    SiteCertificates data = new SiteCertificates();
                    try
                    {
                        data = api.GetSiteCertificates(site.SiteID);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error retrieving certificates for site {site.Domain}: {ex.Message}");
                        oneOrMoreErrors = true;
                        continue;
                    }

                    foreach (List<SiteCertificate> certificates in data.Data)
                    {
                        foreach (SiteCertificate certificate in certificates)
                        {
                            if (string.IsNullOrEmpty(certificate.Certificate))
                                continue;

                            List<string> formattedCertificates = new List<string>();
                            try
                            {
                                formattedCertificates = FormatAndReorderCertificates(certificate.Certificate);
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"Error formatting and reordering certificate chain for site {site.Domain}, {certificate.KeyAlgorithm}: {ex.Message}");
                                oneOrMoreErrors = true;
                                continue;
                            }

                            inventoryItems.Add(new CurrentInventoryItem()
                            {
                                Alias = site.Domain + "|" + certificate.KeyAlgorithm,
                                Certificates = formattedCertificates,
                                ItemStatus = OrchestratorInventoryItemStatus.Unknown,
                                UseChainLevel = true,
                                PrivateKeyEntry = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string errMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error processing Imperva Inventory job, AccountID {AccountID}. ");
                logger.LogError(errMessage);
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = errMessage };
            }

            try
            {
                submitInventory.Invoke(inventoryItems);
                if (oneOrMoreErrors)
                    return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Warning, JobHistoryId = config.JobHistoryId, FailureMessage = "One or more certificates could not be returned.  Please see the log for more details." };
                else
                    return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Success, JobHistoryId = config.JobHistoryId };
            }
            catch (Exception ex)
            {
                string errMessage = ImpervaException.FlattenExceptionMessages(ex, $"Error returning cerificates for Imperva Inventory job, AccountID {AccountID}");
                return new JobResult() { Result = Keyfactor.Orchestrators.Common.Enums.OrchestratorJobStatusJobResult.Failure, JobHistoryId = config.JobHistoryId, FailureMessage = errMessage };
            }
        }
        private List<string> FormatAndReorderCertificates(string pemChain)
        {
            logger.MethodEntry(LogLevel.Debug);

            if (string.IsNullOrWhiteSpace(pemChain))
                return new List<string>();

            MatchCollection certificateBlocks = Regex.Matches(
                pemChain,
                @"-----BEGIN CERTIFICATE-----\s*(?<data>[A-Za-z0-9+/=\r\n\t ]+)\s*-----END CERTIFICATE-----",
                RegexOptions.IgnoreCase);

            if (certificateBlocks.Count == 0)
            {
                logger.MethodExit(LogLevel.Debug);
                throw new FormatException("The certificate chain does not contain any PEM certificate blocks.");
            }

            X509CertificateParser parser = new X509CertificateParser();
            List<X509Certificate> certificates = new List<X509Certificate>();

            foreach (Match block in certificateBlocks)
            {
                string base64 = Regex.Replace(block.Groups["data"].Value, @"\s", string.Empty);

                try
                {
                    byte[] der = Convert.FromBase64String(base64);
                    X509Certificate certificate = parser.ReadCertificate(der);
                    string fingerprint = Convert.ToHexString(certificate.GetEncoded()).ToUpperInvariant();

                    if (!certificates.Any(existing =>
                        Convert.ToHexString(existing.GetEncoded()).Equals(fingerprint, StringComparison.OrdinalIgnoreCase)))
                        certificates.Add(certificate);
                }
                catch (Exception ex)
                {
                    logger.MethodExit(LogLevel.Debug);
                    throw new FormatException("A PEM certificate block could not be parsed.", ex);
                }
            }

            if (certificates.Count == 0)
            {
                logger.MethodExit(LogLevel.Debug);
                throw new FormatException("The certificate chain does not contain any valid certificates.");
            }

            certificates = X509Utilities.OrderChainCertificates(certificates, PKI.Enums.CertificateCollectionOrder.EndEntityFirst);

            List<string> pemCertificates = new List<string>();
            foreach (X509Certificate certificate in certificates)
            {
                pemCertificates.Add(CryptographicObjectFormatter.PEM.Format(certificate, false));
            }

            logger.MethodExit(LogLevel.Debug);
            return pemCertificates;
        }
    }
}