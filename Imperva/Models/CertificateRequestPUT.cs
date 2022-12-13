// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using System.Collections.Generic;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.Orchestrator.Imperva.Model
{
    internal class CertificateRequestPUT
    {
        [JsonProperty("certificate")]
        internal string Certificate { get; set; }
        [JsonProperty("passphrase")]
        internal string Password { get; set; }
        [JsonProperty("auth_type")]
        internal string AuthType { get { return "RSA"; } }
    }
}
