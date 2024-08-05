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
using System.IO;
using OBS.Model;

namespace OBS.Internal
{
    internal class HttpRequest : IDisposable
    {

        private bool disposed;

        private IDictionary<string, string>? headers;

        private IDictionary<string, string?>? parameters;

        private string url;

        private bool autoClose = true;

        internal string GetUrlWithoutQueries()
        {

            var url = Endpoint;

            var hasBucket = !string.IsNullOrEmpty(BucketName);

            if (hasBucket)
            {
                if (PathStyle)
                {
                    url += "/" + BucketName;
                }
                else
                {
                    var index  = url.IndexOf("//");
                    var prefix = url.Substring(0, index + 2);
                    var suffix = url.Substring(index    + 2);

                    url = prefix + BucketName + "." + suffix;
                }
            }

            if (hasBucket && !string.IsNullOrEmpty(ObjectKey))
            {
                url += "/" + CommonUtil.UrlEncode(ObjectKey, Constants.DefaultEncoding, "/");
            }

            return url;
        }

        public string GetUrl()
        {

            if (!string.IsNullOrEmpty(this.url))
            {
                return this.url;
            }

            var url = Endpoint;

            var hasBucket = !string.IsNullOrEmpty(BucketName);

            if (hasBucket)
            {
                if (PathStyle)
                {
                    url += "/" + BucketName;
                }
                else
                {
                    var index  = url.IndexOf("//");
                    var prefix = url.Substring(0, index + 2);
                    var suffix = url.Substring(index    + 2);

                    url = prefix + BucketName + "." + suffix;
                }
            }

            if (hasBucket && !string.IsNullOrEmpty(ObjectKey))
            {
                url += "/" + CommonUtil.UrlEncode(ObjectKey, Constants.DefaultEncoding, "/");
            }

            var paramString = CommonUtil.ConvertParamsToString(Params);
            if (!string.IsNullOrEmpty(paramString))
            {
                url += "?" + paramString;
            }

            this.url = url;

            return this.url;
        }

        public string Endpoint { get; set; }


        public bool PathStyle { get; set; }


        public string BucketName { get; set; }

        public string ObjectKey { get; set; }

        public bool AutoClose
        {
            get { return autoClose; }
            set { autoClose = value; }
        }

        public string GetHost(string endpoint)
        {
            var ub   = new UriBuilder(endpoint);
            var host = ub.Host;
            if (ub.Port != 443 && ub.Port != 80)
            {
                host += ":" + ub.Port;
            }

            if (!string.IsNullOrEmpty(BucketName) && !PathStyle)
            {
                host = BucketName + "." + host;
            }

            return host;
        }

        public HttpVerb Method { get; set; }

        public virtual Stream? Content { get; set; }

        public IDictionary<string, string> Headers
        {
            get { return headers ?? (headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)); }
            internal set { headers = value; }
        }

        public IDictionary<string, string?> Params
        {
            get => parameters ??= new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
            internal set => parameters = value;
        }


        public bool IsRepeatable
        {
            get { return Content == null || Content.CanSeek; }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if (Content != null)
                {
                    if (AutoClose)
                    {
                        Content.Close();
                    }

                    Content = null;
                }

                disposed = true;
            }
        }

    }
}
