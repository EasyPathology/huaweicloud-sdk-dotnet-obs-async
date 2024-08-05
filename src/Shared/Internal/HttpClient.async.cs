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
using OBS.Internal.Log;
using OBS.Model;
using System;
using System.IO;
using System.Net;

namespace OBS.Internal
{
    internal partial class HttpClient
    {
        internal HttpObsAsyncResult BeginPerformRequest(HttpRequest request, HttpContext context, 
            AsyncCallback callback, object state)
        {
            PrepareRequestAndContext(request, context);
            var result = new HttpObsAsyncResult(callback, state);
            result.HttpRequest          = request;
            result.HttpContext          = context;
            result.OriginPos            = request.Content is { CanSeek: true } ? request.Content.Position : -1L;
            result.RetryCount           = 0;
            result.RequestStartDateTime = DateTime.Now;
            BeginDoRequest(result);
            return result;
        }

        internal HttpResponse EndPerformRequest(HttpObsAsyncResult result)
        {
            HttpResponse response = null;
            var request = result.HttpRequest;
            var context = result.HttpContext;
            var maxErrorRetry = context.ObsConfig.MaxErrorRetry;
            var originPos = result.OriginPos;
            try
            {
                response = context.ObsConfig.AsyncSocketTimeout < 0 ? result.Get() : result.Get(context.ObsConfig.AsyncSocketTimeout);
                new MergeResponseHeaderHandler(GetIHeaders(context));
                var statusCode = Convert.ToInt32(response.StatusCode);
                new MergeResponseHeaderHandler(GetIHeaders(context)).Handle(response);

                if (LoggerMgr.IsDebugEnabled)
                {
                    LoggerMgr.Debug(
                        $"Response with statusCode {statusCode} and headers {CommonUtil.ConvertHeadersToString(response.Headers)}");
                }

                switch (statusCode)
                {
                    case >= 300 and < 400 when statusCode != 304:
                    {
                        if (!response.Headers.TryGetValue(Constants.CommonHeaders.Location, value: out var location))
                            throw ParseObsException(response, "Try to redirect, but location is null!", context);
                        if (string.IsNullOrEmpty(location))
                            throw ParseObsException(response, "Try to redirect, but location is null!", context);
                        if (location.IndexOf("?") < 0)
                        {
                            location += "?" + CommonUtil.ConvertParamsToString(request.Params);
                        }
                        if (LoggerMgr.IsWarnEnabled)
                        {
                            LoggerMgr.Warn($"Redirect to {location}");
                        }
                        context.RedirectLocation = location;
                        result.RetryCount--;
                        if (ShouldRetry(request, null, result.RetryCount, maxErrorRetry))
                        {
                            PrepareRetry(request, response, result.RetryCount, originPos, true);
                            result.Reset();
                            BeginDoRequest(result);
                            return EndPerformRequest(result);
                        }

                        if (result.RetryCount > maxErrorRetry)
                        {
                            throw ParseObsException(response, "Exceeded 3xx redirect limit", context);
                        }
                        throw ParseObsException(response, "Try to redirect, but location is null!", context);
                    }
                    case >= 400 and < 500:
                    case 304:
                    {
                        var exception = ParseObsException(response, "Request error", context);
                        if (!Constants.RequestTimeout.Equals(exception.ErrorCode)) throw exception;
                        if (ShouldRetry(request, null, result.RetryCount, maxErrorRetry))
                        {
                            if (LoggerMgr.IsWarnEnabled)
                            {
                                LoggerMgr.Warn("Retrying connection that failed with RequestTimeout error");
                            }
                            PrepareRetry(request, response, result.RetryCount, originPos, true);
                            result.Reset();
                            BeginDoRequest(result);
                            return EndPerformRequest(result);
                        }

                        if (result.RetryCount > maxErrorRetry && LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Exceeded maximum number of retries for RequestTimeout errors");
                        }
                        throw exception;
                    }
                    case >= 500 when ShouldRetry(request, null, result.RetryCount, maxErrorRetry):
                        PrepareRetry(request, response, result.RetryCount, originPos, true);
                        result.Reset();
                        BeginDoRequest(result);
                        return EndPerformRequest(result);
                    case >= 500:
                    {
                        if (result.RetryCount > maxErrorRetry && LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Encountered too many 5xx errors");
                        }
                        throw ParseObsException(response, "Request error", context);
                    }
                }

                foreach (var handler in context.Handlers)
                {
                    handler.Handle(response);
                }
                return response;
            }
            catch (Exception ex)
            {
                try
                {
                    if (ex is ObsException)
                    {
                        if (LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Rethrowing as a ObsException error in EndPerformRequest", ex);
                        }
                        throw ex;
                    }
                    else
                    {
                        if (ShouldRetry(request, ex, result.RetryCount, maxErrorRetry))
                        {
                            PrepareRetry(request, response, result.RetryCount, originPos, true);
                            result.Reset();
                            BeginDoRequest(result);
                            return EndPerformRequest(result);
                        }
                        else if (result.RetryCount > maxErrorRetry && LoggerMgr.IsWarnEnabled)
                        {
                            LoggerMgr.Warn("Too many errors excced the max error retry count", ex);
                        }
                        if (LoggerMgr.IsErrorEnabled)
                        {
                            LoggerMgr.Error("Rethrowing as a ObsException error in PerformRequest", ex);
                        }
                        throw ParseObsException(response, ex.Message, ex, result.HttpContext);
                    }
                }
                finally
                {
                    CommonUtil.CloseIDisposable(response);
                }
            }
        }

        private void BeginDoRequest(HttpObsAsyncResult asyncResult)
        {
            var httpRequest = asyncResult.HttpRequest;
            var context = asyncResult.HttpContext;
            if (!context.SkipAuth)
            {
                GetSigner(context).DoAuth(httpRequest, context, GetIHeaders(context));
            }

            if (!context.ObsConfig.KeepAlive && !httpRequest.Headers.ContainsKey(Constants.CommonHeaders.Connection))
            {
                httpRequest.Headers.Add(Constants.CommonHeaders.Connection, "Close");
            }

            var request = HttpWebRequestFactory.BuildWebRequest(httpRequest, context);
            asyncResult.HttpWebRequest = request;
            asyncResult.HttpStartDateTime = DateTime.Now;


            if (httpRequest.Method == HttpVerb.PUT ||
                httpRequest.Method == HttpVerb.POST || httpRequest.Method == HttpVerb.DELETE)
            {
                BeginSetContent(asyncResult);
            }
            else
            {
                asyncResult.Continue(EndGetResponse);
            }
        }

        private void BeginSetContent(HttpObsAsyncResult asyncResult)
        {
            var webRequest = asyncResult.HttpWebRequest;
            var httpRequest = asyncResult.HttpRequest;

            long userSetContentLength = -1;
            if (httpRequest.Headers.ContainsKey(Constants.CommonHeaders.ContentLength))
            {
                userSetContentLength = long.Parse(httpRequest.Headers[Constants.CommonHeaders.ContentLength]);
            }

            if (userSetContentLength >= 0)
            {
                webRequest.ContentLength = userSetContentLength;
                if (webRequest.ContentLength > Constants.DefaultStreamBufferThreshold)
                {
                    webRequest.AllowWriteStreamBuffering = false;
                }
            }
            else
            {
                webRequest.SendChunked = true;
                webRequest.AllowWriteStreamBuffering = false;
            }

            webRequest.BeginGetRequestStream(EndGetRequestStream, asyncResult);
        }

        private void EndGetRequestStream(IAsyncResult ar)
        {
            var asyncResult = (ar.AsyncState as HttpObsAsyncResult)!;
            var webRequest  = asyncResult.HttpWebRequest;
            var obsConfig   = asyncResult.HttpContext.ObsConfig;
            var data        = asyncResult.HttpRequest.Content ?? new MemoryStream();
            try
            {
                using (var requestStream = webRequest.EndGetRequestStream(ar))
                {
                    ObsCallback callback = delegate
                    {
                        asyncResult.IsTimeout = false;
                    };
                    if (!webRequest.SendChunked)
                    {
                        CommonUtil.WriteTo(data, requestStream, webRequest.ContentLength, obsConfig.BufferSize, callback);
                    }
                    else
                    {
                        CommonUtil.WriteTo(data, requestStream, obsConfig.BufferSize, callback);
                    }
                }
                asyncResult.Continue(EndGetResponse);
            }
            catch (Exception e)
            {
                asyncResult.Abort(e);
            }
        }

        private void EndGetResponse(IAsyncResult ar)
        {
            var asyncResult = (ar.AsyncState as HttpObsAsyncResult)!;
            asyncResult.IsTimeout = false;
            try
            {
                var httpResponse = new HttpResponse(asyncResult.HttpWebRequest.EndGetResponse(ar) as HttpWebResponse);
                asyncResult.Set(httpResponse);
            }
            catch (WebException ex)
            {
                if (ex.Response is not HttpWebResponse response)
                {
                    asyncResult.Abort(ex);
                }
                else
                {
                    asyncResult.Set(new HttpResponse(ex, asyncResult.HttpWebRequest));
                }
            }
            catch (Exception ex)
            {
                asyncResult.Abort(ex);
            }
            finally
            {
                if (LoggerMgr.IsInfoEnabled)
                {
                    LoggerMgr.Info(
                        $"Send http request end, cost {(DateTime.Now.Ticks - asyncResult.HttpStartDateTime.Ticks) / 10000} ms");
                }
            }
        }


    }

    internal class HttpObsAsyncResult : ObsAsyncResult<HttpResponse>
    {
        public HttpObsAsyncResult(AsyncCallback callback, object state) : base(callback, state)
        {
            IsTimeout = true;
        }

        public object AdditionalState { get; set; }

        public HttpRequest HttpRequest { get; set; }

        public HttpContext HttpContext { get; set; }

        public HttpWebRequest? HttpWebRequest { get; set; }

        public long OriginPos { get; set; }

        public int RetryCount { get; set; }

        public bool IsTimeout { get; set; }

        public DateTime HttpStartDateTime { get; set; }

        public DateTime RequestStartDateTime { get; set; }

        public void Reset()
        {
            Reset(null);
            RetryCount++;
        }

        public override void Reset(AsyncCallback callback)
        {
            base.Reset(callback);
            HttpWebRequest = null;
        }

        public override HttpResponse Get(int millisecondsTimeout)
        {
            if (!_isCompleted)
            {
                while (!_event.WaitOne(millisecondsTimeout))
                {
                    if (IsTimeout)
                    {
                        throw new TimeoutException("Socket timeout");
                    }
                    IsTimeout = true;
                }
            }

            if (_exception != null)
            {
                throw _exception;
            }
            return _result;
        }

        public void Abort()
        {
            if (HttpWebRequest == null) return;
            try
            {
                HttpWebRequest.Abort();
            }
            catch (Exception ex)
            {
                LoggerMgr.Error(ex.Message, ex);
            }
        }

        public void Abort(Exception? ex)
        {
            Abort();
            if(ex != null)
            {
                Set(ex);
            }
        }

        public void Continue(AsyncCallback callback)
        {
            try
            {
                HttpWebRequest?.BeginGetResponse(callback, this);
            }
            catch (Exception ex)
            {
                Abort(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                base.Dispose(disposing);
                AdditionalState = null;
                CommonUtil.CloseIDisposable(HttpRequest);
            }
        }
    }


}
