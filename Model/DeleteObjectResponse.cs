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

namespace OBS.Model
{
    /// <summary>
    /// 删除对象的响应结果。
    /// </summary>
    public class DeleteObjectResponse : ObsWebServiceResponse
    {

        /// <summary>
        ///标识删除的对象是否是删除标记。
        /// </summary>
        public bool DeleteMarker
        {
            get;
            internal set;
        }

        /// <summary>
        /// 待删除对象的版本号。
        /// </summary>
        public string VersionId
        {
            get;
            internal set;
        }

    }
}
    
