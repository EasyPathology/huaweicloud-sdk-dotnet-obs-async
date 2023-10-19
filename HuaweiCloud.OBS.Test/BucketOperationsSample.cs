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
using OBS;
using OBS.Model;

namespace ObsDemo
{
    /// <summary>
    /// This sample demonstrates how to do bucket-related operations
    /// (such as do bucket ACL/CORS/Lifecycle/Logging/Website/Location/Tagging)
    /// on OBS using the OBS SDK for .NET
    /// </summary>
    class BucketOperationsSample
    {

        private static string endpoint = "https://your-endpoint";
        private static string AK = "*** Provide your Access Key ***";
        private static string SK = "*** Provide your Secret Key ***";

        private static ObsClient client;


        private static string bucketName = "my-obs-bucket-demo";
        private static string objectName = "my-obs-object-key-demo";

        [Test]
        public static void Test()
        {

            client = new ObsClient(AK, SK, endpoint);

            //create bucket
            CreateBucket();

            Console.ReadKey();

            //head bucket
            HeadBucket();

            //list buckets
            ListBuckets();

            //list objects
            ListObjects();

            //list objects（multi-version）
            ListVersions();

            //get bucket metadata
            GetBucketMetadata();

            //get bucket location
            GetBucketLocation();

            //get bucket  storageinfo
            GetBucketStorageInfo();

            //set bucket quota
            SetBucketQuota();

            //get bucket quota
            GetBucketQuota();

            //set bucket acl
            SetBucketACL();

            //get buclet acl
            GetBucketACL();

            //set bucket lifecycle
            SetBucketLifecycle();

            //get bucket lifecycle
            GetBucketLifecycle();

            //delete bucket lifecycle
            DeleteBucketLifecycle();

            //set bucket website
            SetBucketWebsite();

            //get bucket website
            GetBucketWebsite();

            //delete bucket website
            DeleteBucketWebsite();

            //set bucket version status
            SetBucketVersioning();

            //get bucket version status
            GetBucketVersioning();

            //set bucket cors
            SetBucketCors();

            //get bucket cors
            GetBucketCors();

            //delete bucket cors
            DeleteBucketCors();

            //set bucket logging
            SetBucketLogging();

            //get bucket logging
            GetBucketLogging();

            //delete bucket logging
            DeleteBucketLogging();

            //set bucket tagging
            SetBucketTagging();

            //get bucket tagging
            GetBucketTagging();

            //delete bucket tagging
            DeleteBucketTagging();

            //set bucket nontification
            SetBucketNotification();

            //get bucket nontification
            GetBucketNotification();

            //delete bucket nontification
            DeleteBucketNotification();

            //list multipart uploads
            ListMultipartUploads();

            //set bucket storage policy
            SetBucketStoragePolicy();

            //get bucket storage policy
            GetBucketStoragePolicy();

            //set bucket policy
            SetBucketPolicy();

            //get bucket policy
            GetBucketPolicy();

            //delete bucket policy
            DeleteBucketPolicy();


            //delete bucket
            DeleteBucket();

            Console.WriteLine("bucket operation end...\n\n");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

        }


        #region CreateBucket
        static void CreateBucket()
        {
            try
            {
                var request = new CreateBucketRequest()
                {
                    BucketName = bucketName
                };
                var response = client.CreateBucket(request);

                Console.WriteLine("Create bucket response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when create a bucket.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region HeadBucket
        static void HeadBucket()
        {
            try
            {
                var request = new HeadBucketRequest()
                {
                    BucketName = bucketName
                };
                var response = client.HeadBucket(request);

                Console.WriteLine("Head bucket response: {0}", response);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when head bucket.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region ListBuckets
        static void ListBuckets()
        {
            try
            {
                var request = new ListBucketsRequest();

                var response = client.ListBuckets(request);
                foreach (var bucket in response.Buckets)
                {
                    Console.WriteLine("Bucket name is : {0}", bucket.BucketName);
                    Console.WriteLine("Bucket creationDate is: {0}", bucket.CreationDate.ToString());
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when list buckets.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucket
        private static void DeleteBucket()
        {
            try
            {
                var request = new DeleteBucketRequest()
                {
                    BucketName = bucketName
                };
                var response = client.DeleteBucket(request);
                Console.WriteLine("Delete bucket response: {0}" + response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete a bucket.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region ListObjects
        static void ListObjects()
        {
            try
            {
                var request = new ListObjectsRequest()
                {
                    BucketName = bucketName
                };
                var response = client.ListObjects(request);
                Console.WriteLine("Listing Objects response: {0}" + response.StatusCode);
                foreach (var entry in response.ObsObjects)
                {
                    Console.WriteLine("key = {0} size = {1}", entry.ObjectKey, entry.Size);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when list objects.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region ListVersions
        private static void ListVersions()
        {
            try
            {
                var request = new ListVersionsRequest()
                {
                    BucketName = bucketName,
                    MaxKeys = 10,
                    Delimiter = "delimiter",
                    Prefix = "prefix",
                    KeyMarker = "keyMarker"
                };
                var response = client.ListVersions(request);
                Console.WriteLine("ListVersions response: {0}", response.StatusCode);

                foreach (var objectVersion in response.Versions)
                {
                    Console.WriteLine("ListVersions response Versions Key: {0}", objectVersion.ObjectKey);
                    Console.WriteLine("ListVersions response Versions VersionId: {0}", objectVersion.VersionId);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when list versions.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketMetadata
        private static void GetBucketMetadata()
        {
            try
            {
                List<string> headers = new List<string>();
                headers.Add("x-obs-header");

                var request = new GetBucketMetadataRequest()
                {
                    BucketName = bucketName,
                    Origin = "http://www.a.com",
                    AccessControlRequestHeaders = headers,
                };
                var response = client.GetBucketMetadata(request);
                Console.WriteLine("GetBucketMetadata response: {0}", response.StatusCode);
                foreach (var header in response.Headers)
                {
                    Console.WriteLine("the header {0}: {1}", header.Key, header.Value);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket metadata.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketLocation
        static void GetBucketLocation()
        {
            try
            {
                var request = new GetBucketLocationRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketLocation(request);

                Console.WriteLine("Get bucket location response: {0}", response.StatusCode);
                Console.WriteLine("Bucket Location: {0}", response.Location);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket location.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketStorageInfo
        private static void GetBucketStorageInfo()
        {
            try
            {
                var request = new GetBucketStorageInfoRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketStorageInfo(request);
                Console.WriteLine("GetBucketStorageInfo response response: " + response.StatusCode);
                Console.WriteLine("Object Number={0}", response.ObjectNumber);
                Console.WriteLine("Size={0}", response.Size);
            }
            catch (ObsException ex)
            {
                Console.WriteLine(string.Format("Exception errorcode: {0}, when get bucket storage info.", ex.ErrorCode));
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketQuota
        private static void SetBucketQuota()
        {
            try
            {
                var request = new SetBucketQuotaRequest()
                {
                    BucketName = bucketName,
                    StorageQuota = 0
                };
                var response = client.SetBucketQuota(request);
                Console.WriteLine("Set bucket Quota response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket quota.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketQuota
        private static void GetBucketQuota()
        {
            try
            {
                var request = new GetBucketQuotaRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketQuota(request);
                Console.WriteLine("Get bucket Quota response: {0}" + response.StatusCode);
                Console.WriteLine("Bucket StorageQuota: {0}", response.StorageQuota);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket quota.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketACL
        static void SetBucketACL()
        {
            try
            {

                var owner = new Owner
                {
                    DisplayName = "ownername",
                    Id = "ownerid",
                };


                var grant = new Grant
                {

                    Grantee = new CanonicalGrantee()
                    {
                        DisplayName = "granteename",
                        Id = "granteeid",
                    },

                    Permission = PermissionEnum.FullControl,
                };

                List<Grant> Grants = new List<Grant>();
                Grants.Add(grant);
                var accessControlList = new AccessControlList
                {
                    Owner = owner,
                    Grants = Grants,
                };

                var request = new SetBucketAclRequest()
                {
                    BucketName = bucketName,
                    AccessControlList = accessControlList
                };

                var response = client.SetBucketAcl(request);

                Console.WriteLine("SetBucketACL response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket acl.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketACL
        static void GetBucketACL()
        {
            try
            {
                var request = new GetBucketAclRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketAcl(request);
                Console.WriteLine("Get bucket acl response: {0}", response.StatusCode);
                foreach (var grant in response.AccessControlList.Grants)
                {
                    Console.WriteLine("Grant permission: {0}", grant.Permission);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine(string.Format("Exception errorcode: {0}, when get bucket acl.", ex.ErrorCode));
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketLifecycle
        private static void SetBucketLifecycle()
        {
            try
            {
                var request = new SetBucketLifecycleRequest()
                {
                    BucketName = bucketName,
                    Configuration = new LifecycleConfiguration(),
                };

                var rule1 = new LifecycleRule
                {
                    Id         = "rule1",
                    Prefix     = "prefix",
                    Status     = RuleStatusEnum.Enabled,
                    Expiration =
                    {
                        Days = 30
                    }
                };

                var transition = new Transition()
                {
                    Date = new DateTime(2018, 12, 30, 0, 0, 0),
                    StorageClass = StorageClassEnum.Warm
                };
                rule1.Transitions.Add(transition);

                var noncurrentVersionTransition = new NoncurrentVersionTransition()
                {
                    NoncurrentDays = 30,
                    StorageClass = StorageClassEnum.Cold,
                };
                rule1.NoncurrentVersionTransitions.Add(noncurrentVersionTransition);

                rule1.NoncurrentVersionExpiration.NoncurrentDays = 30;

                request.Configuration.Rules.Add(rule1);

                var response = client.SetBucketLifecycle(request);

                Console.WriteLine("Set bucket lifecycle response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket lifecycle.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketLifecycle
        private static void GetBucketLifecycle()
        {
            try
            {
                var request = new GetBucketLifecycleRequest()
                {
                    BucketName = bucketName,
                };
                var response = client.GetBucketLifecycle(request);
                Console.WriteLine("Get bucket lifecycle response: {0}", response.StatusCode);

                foreach (var lifecycleRule in response.Configuration.Rules)
                {
                    Console.WriteLine("Lifecycle rule id: {0}", lifecycleRule.Id);
                    Console.WriteLine("Lifecycle rule prefix: {0}", lifecycleRule.Prefix);
                    Console.WriteLine("Lifecycle rule status: {0}", lifecycleRule.Status);
                    if (null != lifecycleRule.Expiration)
                    {
                        Console.WriteLine("expiration days: {0}", lifecycleRule.Expiration.Days);
                    }
                    if (null != lifecycleRule.NoncurrentVersionExpiration)
                    {
                        Console.WriteLine("NoncurrentVersionExpiration NoncurrentDays: {0}", lifecycleRule.NoncurrentVersionExpiration.NoncurrentDays);
                    }
                    if (null != lifecycleRule.Transitions)
                    {
                        foreach (var transition in lifecycleRule.Transitions)
                        {
                            Console.WriteLine("Transition Days: {0}", transition.Days.ToString());
                            Console.WriteLine("Transition StorageClass: {0}", transition.StorageClass);
                        }
                    }
                    if (null != lifecycleRule.NoncurrentVersionTransitions)
                    {
                        foreach (var nontransition in lifecycleRule.NoncurrentVersionTransitions)
                        {
                            Console.WriteLine("NoncurrentVersionTransition NoncurrentDays: {0}", nontransition.NoncurrentDays.ToString());
                            Console.WriteLine("NoncurrentVersionTransition StorageClass: {0}", nontransition.StorageClass);
                        }
                    }
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket lifecycle.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketLifecycle
        private static void DeleteBucketLifecycle()
        {
            try
            {
                var request = new DeleteBucketLifecycleRequest()
                {
                    BucketName = bucketName,
                };
                var response = client.DeleteBucketLifecycle(request);
                Console.WriteLine("Delete bucket lifecycle response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket lifecycle.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketWebsite
        private static void SetBucketWebsite()
        {
            try
            {
                var request = new SetBucketWebsiteRequest
                {
                    BucketName    = bucketName,
                    Configuration = new WebsiteConfiguration
                    {
                        IndexDocument = "index.html",
                        ErrorDocument = "error.html"
                    }
                };

                var routingRule = new RoutingRule
                {
                    Redirect = new Redirect
                    {
                        HostName             = "www.example.com",
                        HttpRedirectCode     = "305",
                        Protocol             = ProtocolEnum.Http,
                        ReplaceKeyPrefixWith = "replacekeyprefix"
                    },
                    Condition = new Condition
                    {
                        HttpErrorCodeReturnedEquals = "404",
                        KeyPrefixEquals             = "keyprefix"
                    }
                };
                request.Configuration.RoutingRules = new List<RoutingRule>();
                request.Configuration.RoutingRules.Add(routingRule);

                var response = client.SetBucketWebsiteConfiguration(request);
                Console.WriteLine("Set bucket website response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket website.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region  GetBucketWebsite
        private static void GetBucketWebsite()
        {
            try
            {
                var request = new GetBucketWebsiteRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketWebsite(request);

                Console.WriteLine("GetBucketWebsite response: {0}", response.StatusCode);
                Console.WriteLine("GetBucketWebsite website configuration error document: {0}", response.Configuration.ErrorDocument);
                Console.WriteLine("GetBucketWebsite website configuration index document: {0}", response.Configuration.ErrorDocument);

            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket website.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketWebsite
        private static void DeleteBucketWebsite()
        {
            try
            {
                var request = new DeleteBucketWebsiteRequest()
                {
                    BucketName = bucketName
                };
                var response = client.DeleteBucketWebsite(request);
                Console.WriteLine("DeleteBucketWebsite response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket website.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketVersioning
        private static void SetBucketVersioning()
        {
            try
            {
                var versionConfig = new VersioningConfiguration()
                {
                    Status = VersionStatusEnum.Enabled
                };
                var request = new SetBucketVersioningRequest()
                {
                    BucketName = bucketName,
                    Configuration = versionConfig
                };
                var response = client.SetBucketVersioning(request);

                Console.WriteLine("PutBucketVersioning response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine(string.Format("Exception errorcode: {0}, when set bucket versioning.", ex.ErrorCode));
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketVersioning
        private static void GetBucketVersioning()
        {
            try
            {
                var request = new GetBucketVersioningRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketVersioning(request);

                Console.WriteLine("GetBucketVersioning response: {0}", response.StatusCode);
                Console.WriteLine("GetBucketVersioning version status: {0}", response.Configuration.Status);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket versioning", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketCors
        private static void SetBucketCors()
        {
            try
            {
                var corsConfig = new CorsConfiguration();

                var rule = new CorsRule
                {
                    Id = "20180520"
                };
                rule.AllowedOrigins.Add("http://www.a.com");
                rule.AllowedOrigins.Add("http://www.b.com");
                rule.AllowedHeaders.Add("Authorization");
                rule.AllowedMethods.Add(HttpVerb.GET);
                rule.AllowedMethods.Add(HttpVerb.PUT);
                rule.AllowedMethods.Add(HttpVerb.POST);
                rule.AllowedMethods.Add(HttpVerb.DELETE);
                rule.AllowedMethods.Add(HttpVerb.HEAD);
                rule.ExposeHeaders.Add("x-obs-test1");
                rule.ExposeHeaders.Add("x-obs-test2");
                rule.MaxAgeSeconds = 100;

                corsConfig.Rules.Add(rule);

                var request = new SetBucketCorsRequest()
                {
                    BucketName = bucketName,
                    Configuration = corsConfig,
                };

                var response = client.SetBucketCors(request);
                Console.WriteLine("SetBucketCors response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine(string.Format("Exception errorcode: {0}, when set bucket cors.", ex.ErrorCode));
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketCors
        private static void GetBucketCors()
        {
            try
            {
                var request = new GetBucketCorsRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketCors(request);

                Console.WriteLine("GetBucketCors response: {0}", response.StatusCode);

                foreach (var rule in response.Configuration.Rules)
                {
                    Console.WriteLine("rule id is: {0}\n", rule.Id);
                    foreach (var alowOrigin in rule.AllowedOrigins)
                    {
                        Console.WriteLine("alowOrigin is: {0}\n", alowOrigin);
                    }
                    foreach (var alowHeader in rule.AllowedHeaders)
                    {
                        Console.WriteLine("alowHeader is: {0}\n", alowHeader);
                    }
                    foreach (var alowMethod in rule.AllowedMethods)
                    {
                        Console.WriteLine("alowMethod is: {0}\n", alowMethod);
                    }
                    foreach (var exposeHeader in rule.ExposeHeaders)
                    {
                        Console.WriteLine("exposeHeader is: {0}\n", exposeHeader);
                    }
                    Console.WriteLine("rule maxAgeSeconds is: {0}\n", rule.MaxAgeSeconds);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine(string.Format("Exception errorcode: {0}, when get bucket cors.", ex.ErrorCode));
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketCors
        private static void DeleteBucketCors()
        {
            try
            {
                var request = new DeleteBucketCorsRequest()
                {
                    BucketName = bucketName
                };
                var response = client.DeleteBucketCors(request);
                Console.WriteLine("Delete bucket cors response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket cors.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketTagging
        private static void SetBucketTagging()
        {
            try
            {
                List<Tag> TagList = new List<Tag>();
                var       tag1    = new Tag
                {
                    Key   = "tag1",
                    Value = "value1"
                };

                var tag2 = new Tag
                {
                    Key   = "tag2",
                    Value = "value2"
                };

                TagList.Add(tag1);
                TagList.Add(tag2);

                var request = new SetBucketTaggingRequest()
                {
                    BucketName = bucketName,
                    Tags = TagList
                };

                var response = client.SetBucketTagging(request);

                Console.WriteLine("SetBucketTagging response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errocode: {0}, when set bucket tagging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketTagging
        private static void GetBucketTagging()
        {
            try
            {
                var request = new GetBucketTaggingRequest()
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketTagging(request);

                Console.WriteLine("Get bucket Tagging response: {0}", response.StatusCode);
                foreach (var tag in response.Tags)
                {
                    Console.WriteLine("Get bucket Tagging response Key: {0}" + tag.Key);
                    Console.WriteLine("Get bucket Tagging response Value:{0} " + tag.Value);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket tagging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketTagging
        private static void DeleteBucketTagging()
        {
            try
            {
                var request = new DeleteBucketTaggingRequest()
                {
                    BucketName = bucketName
                };
                var response = client.DeleteBucketTagging(request);

                Console.WriteLine("DeleteBucketTagging response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket tagging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketLogging
        static void SetBucketLogging()
        {
            try
            {

                var acl = new AccessControlList
                {
                    Owner = new Owner
                    {
                        Id = "domainId"
                    }
                };
                var item = new Grant
                {
                    Permission = PermissionEnum.FullControl
                };
                var group = new GroupGrantee
                {
                    GroupGranteeType = GroupGranteeEnum.LogDelivery
                };
                item.Grantee = group;
                acl.Grants.Add(item);

                var setAclRequest = new SetBucketAclRequest
                {
                    BucketName = "targetbucketname",
                    AccessControlList = acl,
                };


                var setAclResponse = client.SetBucketAcl(setAclRequest);
                Console.WriteLine("Set bucket target acl response: {0}", setAclResponse.StatusCode);

                var loggingConfig = new LoggingConfiguration
                {
                    TargetBucketName = "targetbucketname",
                    TargetPrefix     = "targetPrefix"
                };

                var request = new SetBucketLoggingRequest()
                {
                    BucketName = bucketName,
                    Configuration = loggingConfig
                };

                var response = client.SetBucketLogging(request);

                Console.WriteLine("Set bucket logging status: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket logging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketLogging
        static void GetBucketLogging()
        {
            try
            {
                var request = new GetBucketLoggingRequest
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketLogging(request);

                Console.WriteLine("TargetBucketName is : " + response.Configuration.TargetBucketName);
                Console.WriteLine("TargetPrefix is : " + response.Configuration.TargetPrefix);
                Console.WriteLine("Get bucket logging response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket logging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketLogging
        static void DeleteBucketLogging()
        {
            try
            {
                var request = new SetBucketLoggingRequest
                {
                    BucketName    = bucketName,
                    Configuration = new LoggingConfiguration()
                };
                var response = client.SetBucketLogging(request);
                Console.WriteLine("Delete bucket logging response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket logging.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion      

        #region SetBucketNotification
        static void SetBucketNotification()
        {
            try
            {
                var filterRule1 = new FilterRule
                {
                    Name  = FilterNameEnum.Prefix,
                    Value = "smn"
                };
                var topicConfiguration1 = new TopicConfiguration
                {
                    Id    = "Id001",
                    Topic = "urn:smn:globrg:35667523534:topic1"
                };
                topicConfiguration1.Events.Add(EventTypeEnum.ObjectCreatedAll);
                topicConfiguration1.FilterRules = new List<FilterRule>();
                topicConfiguration1.FilterRules.Add(filterRule1);

                var filterRule2 = new FilterRule
                {
                    Name  = FilterNameEnum.Suffix,
                    Value = ".jpg"
                };
                var topicConfiguration2 = new TopicConfiguration
                {
                    Id    = "Id002",
                    Topic = "urn:smn:globrg:35667523535:topic2"
                };
                topicConfiguration2.Events.Add(EventTypeEnum.ObjectRemovedAll);
                topicConfiguration2.FilterRules = new List<FilterRule>();
                topicConfiguration2.FilterRules.Add(filterRule2);

                var notificationConfiguration = new NotificationConfiguration
                {
                    TopicConfigurations = new List<TopicConfiguration>()
                };
                notificationConfiguration.TopicConfigurations.Add(topicConfiguration1);
                notificationConfiguration.TopicConfigurations.Add(topicConfiguration2);

                var request = new SetBucketNotificationRequest
                {
                    BucketName = bucketName,
                    Configuration = notificationConfiguration,
                };
                var response = client.SetBucketNotification(request);
                Console.WriteLine("Set bucket notification response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket notification.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketNotification
        static void GetBucketNotification()
        {
            try
            {
                var request = new GetBucketNotificationRequest
                {
                    BucketName = bucketName
                };
                var response = client.GetBucketNotification(request);
                if (response.Configuration.TopicConfigurations.Count > 0)
                {
                    foreach (var topicConfig in response.Configuration.TopicConfigurations)
                    {
                        Console.WriteLine("ID is : {0}", topicConfig.Id);
                        Console.WriteLine("Topic is : {0}", topicConfig.Topic);
                        foreach (var Event in topicConfig.Events)
                        {
                            Console.WriteLine("Event is : {0}", Event);
                        }
                        foreach (var filterRule in topicConfig.FilterRules)
                        {
                            Console.WriteLine("Name is : {0}", filterRule.Name);
                            Console.WriteLine("Value is : {0}", filterRule.Value);
                        }
                    }
                }
                Console.WriteLine("Get bucket notification  response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket notification.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketNotification
        static void DeleteBucketNotification()
        {
            try
            {
                var notificationConfig = new NotificationConfiguration();
                var request = new SetBucketNotificationRequest
                {
                    BucketName = bucketName,
                    Configuration = notificationConfig
                };
                var response = client.SetBucketNotification(request);
                Console.WriteLine("Delete bucket notification  response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket notification.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region ListMultipartUploads
        static void ListMultipartUploads()
        {
            try
            {
                var request = new ListMultipartUploadsRequest()
                {
                    BucketName = bucketName,
                    Delimiter = "delimiter",
                    Prefix = "prefix",
                    KeyMarker = "keymarker",
                    MaxUploads = 10,
                    UploadIdMarker = "uploadidmarker"
                };
                var response = client.ListMultipartUploads(request);
                Console.WriteLine("List multipart uploads response: {0}", response.StatusCode);

                foreach (var multipart in response.MultipartUploads)
                {
                    Console.WriteLine("MultipartUpload object key: {0}", multipart.ObjectKey);
                    Console.WriteLine("MultipartUpload upload id: {0}", multipart.UploadId);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when list multipart uploads.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketStoragePolicy
        static void SetBucketStoragePolicy()
        {
            try
            {
                var request = new SetBucketStoragePolicyRequest()
                {
                    BucketName = bucketName,
                    StorageClass = StorageClassEnum.Standard
                };
                var response = client.SetBucketStoragePolicy(request);

                Console.WriteLine("Set bucket storage policy response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket storage policy.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketStoragePolicy
        static void GetBucketStoragePolicy()
        {
            try
            {
                var request = new GetBucketStoragePolicyRequest()
                {
                    BucketName = bucketName,
                };
                var response = client.GetBucketStoragePolicy(request);

                Console.WriteLine("Get bucket storage policy response: {0}", response.StatusCode);
                Console.WriteLine("Bucket DefaultStorageClass: {0}", response.StorageClass.ToString());
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get bucket storage policy.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetBucketPolicy
        static void SetBucketPolicy()
        {
            try
            {
                var request = new SetBucketPolicyRequest()
                {
                    BucketName = bucketName,
                    ContentMD5 = "md5",
                    Policy = "policy"
                };
                var response = client.SetBucketPolicy(request);

                Console.WriteLine("Set bucket policy response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket policy.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetBucketPolicy
        static void GetBucketPolicy()
        {
            try
            {
                var request = new GetBucketPolicyRequest()
                {
                    BucketName = bucketName,
                };
                var response = client.GetBucketPolicy(request);

                Console.WriteLine("Get bucket policy response: {0}", response.StatusCode);
                Console.WriteLine("Bucket policy: {0}", response.Policy);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set bucket policy.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteBucketPolicy
        static void DeleteBucketPolicy()
        {
            try
            {
                var request = new DeleteBucketPolicyRequest()
                {
                    BucketName = bucketName,
                };
                var response = client.DeleteBucketPolicy(request);

                Console.WriteLine("Delete bucket policy response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete bucket policy.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion



    }

}