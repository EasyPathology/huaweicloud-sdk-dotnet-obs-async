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
using OBS.Model;
using OBS;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace ObsDemo
{
    /// <summary>
    /// This sample demonstrates how to do common operations in temporary signature
    /// way on OBS using the OBS SDK for .NET.
    /// </summary>
    class TemporarySignatureSample
    {

        private static string endpoint = "https://your-endpoint";
        private static string AK = "*** Provide your Access Key ***";
        private static string SK = "*** Provide your Secret Key ***";

        private static ObsClient client;

        private static string bucketName = "my-obs-bucket-demo";
        private static string objectKey = "my-obs-object-key-demo";


        [Test]
        public static void Test()
        {
            // Constructs a obs client instance with your account for accessing OBS
            var config = new ObsConfig
            {
                Endpoint = endpoint
            };
            client = new ObsClient(AK, SK, config);

            // Create bucket
            DoCreateBucket();

            // Set/Get/Delete bucket cors
            DoBucketCorsOperations();

            // Create object
            DoCreateObject();

            // Get object
            DoGetObject();

            // Set/Get object acl
            DoObjectAclOperations();

            // Delete object
            DoDeleteObject();

            // Delete bucket
            //DoDeleteBucket();


            Console.ReadKey();
        }


        private static MethodInfo GetAddHeaderInternal()
        {
            return typeof(WebHeaderCollection).GetMethod("AddInternal", BindingFlags.NonPublic | BindingFlags.Instance,
                            null, new Type[] { typeof(string), typeof(string) }, null);
        }

        private static void GetResponse(HttpVerb method, CreateTemporarySignatureResponse response, String content)
        {

            var webRequest = WebRequest.Create(response.SignUrl) as HttpWebRequest;
            webRequest.Method = method.ToString().ToUpper();


            foreach (var header in response.ActualSignedRequestHeaders)
            {
                GetAddHeaderInternal().Invoke(webRequest.Headers, new object[] { header.Key, header.Value });
                //Console.WriteLine("{0}={1}", header.Key, header.Value);
            }

            if (!string.IsNullOrEmpty(content))
            {
                webRequest.SendChunked = true;
                webRequest.AllowWriteStreamBuffering = false;
                using var requestStream = webRequest.GetRequestStream();
                var       buffer        = Encoding.UTF8.GetBytes(content);
                requestStream.Write(buffer, 0, buffer.Length);
            }


            HttpWebResponse webResponse = null;
            try
            {
                webResponse = webRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                webResponse = ex.Response as HttpWebResponse;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            if(webResponse != null)
            {
                
                if (Convert.ToInt32(webResponse.StatusCode) < 300)
                {
                    Console.WriteLine("Do action successfully with Response Code:" + Convert.ToInt32(webResponse.StatusCode));
                }

                using var dest = new MemoryStream();
                using (var stream = webResponse.GetResponseStream())
                {
                    var buffer = new byte[8192];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        dest.Write(buffer, 0, bytesRead);
                    }

                }
                Console.WriteLine("Response Content:");
                Console.WriteLine(Encoding.UTF8.GetString(dest.ToArray()));
            }

        }

        private static void GetResponse(HttpVerb method, CreateTemporarySignatureResponse response)
        {
            GetResponse(method, response, null);
        }

        private static void DoDeleteBucket()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName = bucketName,
                Method     = HttpVerb.DELETE,
                Expires    = 3600
            };
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Deleting bucket using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);
        }

        private static void DoDeleteObject()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName = bucketName,
                ObjectKey  = objectKey,
                Method     = HttpVerb.DELETE,
                Expires    = 3600
            };
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Deleting object using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);
        }

        private static void DoObjectAclOperations()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName  = bucketName,
                ObjectKey   = objectKey,
                Method      = HttpVerb.PUT,
                Expires     = 3600,
                SubResource = SubResourceEnum.Acl
            };
            request.Headers.Add("x-obs-acl", "public-read");
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Setting object ACL to public-read using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);



            request             = new CreateTemporarySignatureRequest
            {
                BucketName  = bucketName,
                ObjectKey   = objectKey,
                Method      = HttpVerb.GET,
                Expires     = 3600,
                SubResource = SubResourceEnum.Acl
            };
            response            = client.CreateTemporarySignature(request);
            Console.WriteLine("Getting object ACL using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);


        }

        private static void DoGetObject()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName = bucketName,
                ObjectKey  = objectKey,
                Method     = HttpVerb.GET,
                Expires    = 3600
            };
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Getting object using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);
        }

        private static void DoCreateObject()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName = bucketName,
                ObjectKey  = objectKey,
                Method     = HttpVerb.PUT,
                Expires    = 3600
            };
            request.Headers.Add("Content-Type", "text/plain");
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Createing object using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response, "Hello OBS");
        }

        private static void DoBucketCorsOperations()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName  = bucketName,
                Method      = HttpVerb.PUT,
                Expires     = 3600,
                SubResource = SubResourceEnum.Cors
            };
            request.Headers.Add("Content-Type", "application/xml");
            var requestXml = "<CORSConfiguration><CORSRule><AllowedMethod>GET</AllowedMethod><AllowedOrigin>*</AllowedOrigin><AllowedHeader>*</AllowedHeader></CORSRule></CORSConfiguration>";

            request.Headers.Add("Content-MD5", Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(requestXml))));

            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Setting bucket CORS using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response, requestXml);

            request             = new CreateTemporarySignatureRequest
            {
                BucketName  = bucketName,
                Method      = HttpVerb.GET,
                Expires     = 3600,
                SubResource = SubResourceEnum.Cors
            };
            response            = client.CreateTemporarySignature(request);
            Console.WriteLine("Getting bucket CORS using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            GetResponse(request.Method, response);
        }

        private static void DoCreateBucket()
        {
            var request = new CreateTemporarySignatureRequest
            {
                BucketName = bucketName,
                Method     = HttpVerb.PUT,
                Expires    = 3600
            };
            var response = client.CreateTemporarySignature(request);
            Console.WriteLine("Creating bucket using temporary signature url:");
            Console.WriteLine("\t" + response.SignUrl);
            var location = "your location";
            var requestXml = "<CreateBucketConfiguration><Location>" + location + "</Location></CreateBucketConfiguration>";
            GetResponse(request.Method, response);
        }
    }

}
