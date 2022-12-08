using System.Collections.Generic;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.Imperva.Model
{
    internal class Site
    {
        [JsonPropertyName("sites")]
        internal List<SiteDetail> Sites { get; set; }
    }

    internal class SiteDetail
    {
        [JsonProperty("site_id")]
        internal int SiteID { get; set; }
        [JsonProperty("domain")]
        internal string Domain { get; set; }
    }
}
