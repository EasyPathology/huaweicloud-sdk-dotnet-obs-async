﻿/*----------------------------------------------------------------------------------
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
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System;
using System.IO;
using OBS.Internal.Log;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Reflection;
using OBS.Model;

namespace OBS.Internal
{
    internal static class CommonUtil
    {

        private static readonly IDictionary<Type, MethodInfo> TransMethodsHolder = new Dictionary<Type, MethodInfo>();
        private static readonly object TransMethodsHolderLock = new object();

        private static readonly IDictionary<Type, MethodInfo> ParseMethodsHolder = new Dictionary<Type, MethodInfo>();
        private static readonly object ParseMethodsHolderLock = new object();

        private static readonly Regex ChinesePattern = new Regex("[\u4e00-\u9fa5]");
        private static readonly Regex IPPattern = new Regex(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");

        public static void CloseIDisposable(IDisposable disposable)
        {
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ee)
                {
                    if (LoggerMgr.IsErrorEnabled)
                    {
                        LoggerMgr.Error(ee.Message, ee);
                    }
                }
            }
        }

        public static MethodInfo GetTransMethodInfo(Type requestType, object iconvertor)
        {
            MethodInfo info;
            TransMethodsHolder.TryGetValue(requestType, out info);
            if (info == null)
            {
                lock(TransMethodsHolderLock)
                {
                    TransMethodsHolder.TryGetValue(requestType, out info);
                    if (info == null)
                    {
                        info = iconvertor.GetType().GetMethod("Trans", BindingFlags.Public | BindingFlags.Instance, null,
                   new Type[] { requestType }, null);
                        TransMethodsHolder.Add(requestType, info);
                    }
                }
            }

            return info;
        }

        public static MethodInfo GetParseMethodInfo(Type responseType, object iparser)
        {
            if (!ParseMethodsHolder.ContainsKey(responseType))
            {
                lock (ParseMethodsHolderLock)
                {
                    if (!ParseMethodsHolder.ContainsKey(responseType))
                    {
                        var info = iparser.GetType().GetMethod("Parse" + responseType.Name, BindingFlags.Public | BindingFlags.Instance, null,
                   new Type[] { typeof(HttpResponse) }, null);
                        ParseMethodsHolder.Add(responseType, info);
                    }
                }
            }
            return ParseMethodsHolder[responseType];
        }

        public static void RenameHeaders(HttpRequest request, string headerPrefix, string headerMetaPrefix)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, string> header in request.Headers)
            {
                if (string.IsNullOrEmpty(header.Key))
                {
                    continue;
                }

                var key = header.Key.Trim();
                var value = header.Value == null ? "" : header.Value;
                var isValid = false;
                if (key.StartsWith(headerPrefix, StringComparison.OrdinalIgnoreCase) || key.StartsWith(Constants.ObsHeaderPrefix, StringComparison.OrdinalIgnoreCase) || Constants.AllowedRequestHttpHeaders.Contains(key.ToLower()))
                {
                    isValid = true;
                }
                else if (request.Method == HttpVerb.POST || request.Method == HttpVerb.PUT)
                {
                    key = headerMetaPrefix + key;
                    isValid = true;
                }

                if (isValid)
                {
                    if(key.StartsWith(headerMetaPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        key = UrlEncode(key, true);
                    }
                    headers.Add(key, UrlEncode(value, true));
                }
            }

            request.Headers = headers;
        }

        public static void WriteTo(Stream src, Stream dest, int bufferSize)
        {
            WriteTo(src, dest, bufferSize, null);
        }

        public static void WriteTo(Stream src, Stream dest, int bufferSize, ObsCallback callback)
        {
            var reqTime = DateTime.Now;
            var buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = src.Read(buffer, 0, buffer.Length)) > 0)
            {
                dest.Write(buffer, 0, bytesRead);
                callback?.Invoke();
            }
            dest.Flush();
            if (LoggerMgr.IsInfoEnabled)
            {
                LoggerMgr.Info($"Write http request stream end, cost {(DateTime.Now.Ticks - reqTime.Ticks) / 10000} ms");
            }
        }

        public static bool IsIP(string endpoint)
        {
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new ArgumentNullException("Endpoint is null");
            }
            var ub = new UriBuilder(endpoint);

            return IPPattern.IsMatch(ub.Host);
        }

        public static void AddHeader(HttpRequest request, string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                request.Headers.Add(key, value);
            }
        }

        public static void AddParam(HttpRequest request, string key, string value)
        {
            if (!string.IsNullOrEmpty(key))
            {
                request.Params.Add(key, value);
            }
        }

        public static long WriteTo(Stream orignStream, Stream destStream, long totalSize, int bufferSize)
        {
            return WriteTo(orignStream, destStream, totalSize, bufferSize, null);
        }

        public static long WriteTo(Stream orignStream, Stream destStream, long totalSize, int bufferSize, ObsCallback callback)
        {
            var reqTime = DateTime.Now;
            var buffer = new byte[bufferSize];

            long alreadyRead = 0;
            while (alreadyRead < totalSize)
            {
                var readSize = orignStream.Read(buffer, 0, bufferSize);
                if (readSize <= 0)
                {
                    break;
                }

                if (alreadyRead + readSize > totalSize)
                {
                    readSize = (int)(totalSize - alreadyRead);
                }
                alreadyRead += readSize;
                destStream.Write(buffer, 0, readSize);
                callback?.Invoke();
            }
            destStream.Flush();

            if (LoggerMgr.IsInfoEnabled)
            {

                LoggerMgr.Info($"Write http request stream end, cost {(DateTime.Now.Ticks - reqTime.Ticks) / 10000} ms");
            }

            return alreadyRead;
        }

        public static string ConvertHeadersToString(IDictionary<string, string> headers)
        {
            var headerString = new StringBuilder();
            headerString.Append("{");
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> h in headers)
                {

                    headerString.Append(h.Key).Append(":").Append(h.Value);
                }

            }
            headerString.Append("}");
            return headerString.ToString();
        }

        public static byte[] HmacSha1(string key, string toSign)
        {
            var byteToSign = Encoding.UTF8.GetBytes(toSign);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var hmac = new HMACSHA1(byteKey);
            return hmac.ComputeHash(byteToSign);
        }

        public static byte[] HmacSha256(byte[] key, string toSign)
        {
            var byteToSign = Encoding.UTF8.GetBytes(toSign);
            var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(byteToSign);
        }

        public static byte[] HmacSha256(string key, string toSign)
        {
            var byteToSign = Encoding.UTF8.GetBytes(toSign);
            var byteKey = Encoding.UTF8.GetBytes(key);
            var hmac = new HMACSHA256(byteKey);
            return hmac.ComputeHash(byteToSign);
        }

        public static string ToHex(byte[] data)
        {
            var sbBytes = new StringBuilder(data.Length * 2);
            foreach (var b in data)
            {
                sbBytes.AppendFormat("{0:x2}", b);
            }
            return sbBytes.ToString();
        }

        public static string HexSha256(string toSign)
        {
            byte[] hash;
            using (SHA256 s256 = new SHA256Managed())
            {
                hash = s256.ComputeHash(Encoding.UTF8.GetBytes(toSign));
                s256.Clear();
            }
            return ToHex(hash);
        }

        public static byte[] Md5(byte[] data)
        {
            return new MD5CryptoServiceProvider().ComputeHash(data);
        }

        public static byte[] Md5(Stream stream)
        {
            return new MD5CryptoServiceProvider().ComputeHash(stream);
        }

        public static string Base64Md5(byte[] data)
        {
            return Convert.ToBase64String(Md5(data));
        }

        public static string Base64Md5(Stream stream)
        {
            if(stream == null)
            {
                return Base64Md5(Encoding.UTF8.GetBytes(""));
            }
            return Convert.ToBase64String(Md5(stream)); 
        }

        public static string ConvertParamsToCanonicalQueryString(List<KeyValuePair<string, string>> kvlist)
        {
            var queryString = new StringBuilder();
            if (kvlist is { Count: > 0 })
            {
                var cnt = kvlist.Count;
                var index = 0;
                foreach (KeyValuePair<string, string> p in kvlist)
                {
                    queryString.Append(UrlEncode(p.Key, Constants.DefaultEncoding, "/")).Append("=").Append(UrlEncode(p.Value == null ? "" : p.Value, Constants.DefaultEncoding));
                    if(index++ != cnt - 1)
                    {
                        queryString.Append("&");
                    }
                }

            }
            return queryString.ToString();
        }

        public static string ConvertParamsToString(IDictionary<string, string> parameters)
        {
            var queryString = new StringBuilder();
            if (parameters is { Count: > 0 })
            {

                var isFirst = true;

                foreach (KeyValuePair<string, string> p in parameters)
                {
                    if (string.IsNullOrEmpty(p.Key))
                    {
                        continue;
                    }

                    if (!isFirst)
                    {
                        queryString.Append("&");
                    }

                    isFirst = false;

                    queryString.Append(UrlEncode(p.Key, Constants.DefaultEncoding, "/"));
                    if (p.Value != null)
                    {
                        queryString.Append("=").Append(UrlEncode(p.Value, Constants.DefaultEncoding, "/"));
                    }
                }

            }
            return queryString.ToString();
        }

        public static string UrlEncode(string uriToEncode, bool chineseOnly)
        {
            if (chineseOnly)
            {
                var sb = new StringBuilder();
                foreach (var c in uriToEncode)
                {
                    if (ChinesePattern.IsMatch(c.ToString()))
                    {
                        sb.Append(UrlEncode(c.ToString(), Constants.DefaultEncoding));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                return sb.ToString();
            }

            return UrlEncode(uriToEncode, Constants.DefaultEncoding);
        }

        public static string UrlEncode(string uriToEncode)
        {
            return UrlEncode(uriToEncode, Constants.DefaultEncoding, null);
        }

        public static string UrlEncode(string uriToEncode, string charset)
        {
            return UrlEncode(uriToEncode, charset, null);
        }

        public static string UrlEncode(string uriToEncode, string charset, string safe)
        {
            if (string.IsNullOrEmpty(uriToEncode))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(charset))
            {
                charset = Constants.DefaultEncoding;

            }

            const string escapeFlag = "%";
            var encodedUri = new StringBuilder(uriToEncode.Length * 2);
            var bytes = Encoding.GetEncoding(charset).GetBytes(uriToEncode);
            foreach (var b in bytes)
            {
                var ch = (char)b;
                if (Constants.AllowedInUrl.IndexOf(ch) != -1)
                {
                    encodedUri.Append(ch);
                }else if(safe != null && safe.IndexOf(ch) != -1)
                {
                    encodedUri.Append(ch);
                }
                else
                {
                    encodedUri.Append(escapeFlag).Append(
                        string.Format(CultureInfo.InvariantCulture, "{0:X2}", (int)b));
                }
            }

            return encodedUri.ToString();
        }


        public static string UrlDecode(string uriToDecode)
        {
            if (!string.IsNullOrEmpty(uriToDecode))
            {
                return Uri.UnescapeDataString(uriToDecode.Replace("+", " "));
            }
            return "";
        }


        public static int? ParseToInt32(string value)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? (int?)null : Convert.ToInt32(value);
            }catch(Exception ex)
            {
                if (LoggerMgr.IsWarnEnabled)
                {
                    LoggerMgr.Warn($"Parse {value} to Int32 failed", ex);
                }
                return null;
            }
        }

        public static long? ParseToInt64(string value)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? (long?)null : Convert.ToInt64(value);
            }
            catch (Exception ex)
            {
                if (LoggerMgr.IsWarnEnabled)
                {
                    LoggerMgr.Warn($"Parse {value} to Int64 failed", ex);
                }
                return null;
            }
        }

        public static DateTime? ParseToDateTime(string value, string format1, string format2)
        {
            try
            {
                return string.IsNullOrEmpty(value) ? (DateTime?)null : DateTime.ParseExact(value, format1, Constants.CultureInfo);
            }
            catch (Exception)
            {
                try
                {
                    return DateTime.ParseExact(value, format2, Constants.CultureInfo);
                }
                catch (Exception ex)
                {
                    if (LoggerMgr.IsWarnEnabled)
                    {
                        LoggerMgr.Warn($"Parse {value} to DateTime failed", ex);
                    }
                    return null;
                }
            }
        }

        public static DateTime? ParseToDateTime(string value)
        {
            return ParseToDateTime(value, Constants.ISO8601DateFormat, Constants.ISO8601DateFormatNoMS);
        }

    }
}