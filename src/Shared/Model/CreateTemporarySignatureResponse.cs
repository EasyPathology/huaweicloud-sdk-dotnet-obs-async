/*----------------------------------------------------------------------------------
// Copyright 2019 Huawei Technologies Co.,Ltd.
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License.  You may obtain a copy of the
// License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations under the License.
//----------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace OBS.Model
{
    /// <summary>
    /// Response to a temporary authentication request
    /// </summary>
    public class CreateTemporarySignatureResponse
    {
        private IDictionary<string, string> actualSignedRequestHeaders;
            
        /// <summary>
        /// Temporarily signed URL
        /// </summary>
        public string SignUrl
        {
            get;
            internal set;
        }

        /// <summary>
        /// Headers used for authentication
        /// </summary>
        public IDictionary<string,string> ActualSignedRequestHeaders
        {
            get {
                
                return actualSignedRequestHeaders ?? (actualSignedRequestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)); }
            internal set { actualSignedRequestHeaders = value; }
        }

    }
}
    


