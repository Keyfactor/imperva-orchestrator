// Copyright 2021 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.Orchestrator.Imperva
{
    public class BaseJob
    {
        internal string ApiURL { get; set; }
        internal string AccountID { get; set; }
        internal string ApiID { get; set; }
        internal string ApiKey { get; set; }

        internal void SetAPIProperties(string apiUrl, string accountID, string apiIDKey)
        {
            string[] properties = storePath.Split(new char[] { '|' });
            if (properties.Length != 2)
                throw new ImpervaException("Invalid Store Password.  Value must a string with 2 values, your Api ID then Api Key separated by '|'");

            ApiURL = apiUrl;
            AccountID = accountID;
            ApiID = properties[0];
            ApiKey = properties[1];
        }
    }
}
