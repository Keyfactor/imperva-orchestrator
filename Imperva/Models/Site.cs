using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Keyfactor.Extensions.Orchestrator.Imperva.Model
{
    internal class Site
    {
        [JsonPropertyName("sites")]
        internal List<SiteDetail> Sites { get; set; }
    }

    internal class SiteDetail
    {
        [JsonPropertyName("site_id")]
        internal int SiteID { get; set; }
        [JsonPropertyName("domain")]
        internal string Domain { get; set; }
    }
}
