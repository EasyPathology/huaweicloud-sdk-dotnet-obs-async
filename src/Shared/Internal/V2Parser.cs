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
using System;
using OBS.Model;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace OBS.Internal
{
    internal class V2Parser : IParser
    {

        protected IHeaders iheaders;

        protected V2Parser(IHeaders iheaders)
        {
            this.iheaders = iheaders;
        }

        public static IParser GetInstance(IHeaders iheaders)
        {
            return new V2Parser(iheaders);
        }

        protected virtual StorageClassEnum? ParseStorageClass(string value)
        {
            return EnumAdaptor.V2StorageClassEnumDict.ContainsKey(value) ? EnumAdaptor.V2StorageClassEnumDict[value] : (StorageClassEnum?)null;
        }

        protected PermissionEnum? ParsePermission(string value)
        {
            return EnumAdaptor.PermissionEnumDict.ContainsKey(value) ? EnumAdaptor.PermissionEnumDict[value] : (PermissionEnum?)null;
        }

        protected virtual GroupGranteeEnum? ParseGroupGrantee(string value)
        {
            return EnumAdaptor.V2GroupGranteeEnumDict.ContainsKey(value) ? EnumAdaptor.V2GroupGranteeEnumDict[value] : (GroupGranteeEnum?)null;
        }

        protected HttpVerb? ParseHttpVerb(string value)
        {
            return EnumAdaptor.HttpVerbEnumDict.ContainsKey(value) ? EnumAdaptor.HttpVerbEnumDict[value] : (HttpVerb?)null;
        }

        protected RuleStatusEnum? ParseRuleStatus(string value)
        {
            return EnumAdaptor.RuleStatusEnumDict.ContainsKey(value) ? EnumAdaptor.RuleStatusEnumDict[value] : (RuleStatusEnum?)null;
        }

        protected VersionStatusEnum? ParseVersionStatusEnum(string value)
        {
            return EnumAdaptor.VersionStatusEnumDict.ContainsKey(value) ? EnumAdaptor.VersionStatusEnumDict[value] : (VersionStatusEnum?)null;
        }

        protected ProtocolEnum? ParseProtocol(string value)
        {
            return EnumAdaptor.ProtocolEnumDict.ContainsKey(value) ? EnumAdaptor.ProtocolEnumDict[value] : (ProtocolEnum?)null;
        }

        protected FilterNameEnum? ParseFilterName(string value)
        {
            return EnumAdaptor.FilterNameEnumDict.ContainsKey(value) ? EnumAdaptor.FilterNameEnumDict[value] : (FilterNameEnum?)null;
        }

        protected virtual EventTypeEnum? ParseEventTypeEnum(string value)
        {
            return EnumAdaptor.V2EventTypeEnumDict.ContainsKey(value) ? EnumAdaptor.V2EventTypeEnumDict[value] : (EventTypeEnum?)null;
        }

        protected virtual string BucketLocationTag
        {
            get
            {
                return "LocationConstraint";
            }
        }

        protected virtual string BucketStorageClassTag
        {
            get
            {
                return "DefaultStorageClass";
            }
        }

        public ListBucketsResponse ParseListBucketsResponse(HttpResponse httpResponse)
        {
            var response = new ListBucketsResponse();

            using var reader        = XmlReader.Create(httpResponse.Content);
            Owner     owner         = null;
            ObsBucket currentBucket = null;
            while (reader.Read())
            {
                if ("Owner".Equals(reader.Name))
                {
                    if (reader.IsStartElement())
                    {
                        owner          = new Owner();
                        response.Owner = owner;
                    }
                }
                else if ("ID".Equals(reader.Name))
                {
                    if (owner != null)
                    {
                        owner.Id = reader.ReadString();
                    }
                }
                else if ("DisplayName".Equals(reader.Name))
                {
                    if (owner != null)
                    {
                        owner.DisplayName = reader.ReadString();
                    }
                }
                else if ("Bucket".Equals(reader.Name))
                {
                    if (reader.IsStartElement())
                    {
                        currentBucket = new ObsBucket();
                        response.Buckets.Add(currentBucket);
                    }
                }
                else if ("Name".Equals(reader.Name))
                {
                    currentBucket.BucketName = reader.ReadString();
                }
                else if ("CreationDate".Equals(reader.Name))
                {
                    currentBucket.CreationDate = CommonUtil.ParseToDateTime(reader.ReadString());
                }
                else if ("Location".Equals(reader.Name))
                {
                    currentBucket.Location = reader.ReadString();
                }
            }

            return response;
        }


        public GetBucketStoragePolicyResponse ParseGetBucketStoragePolicyResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketStoragePolicyResponse();

            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(BucketStorageClassTag))
                {
                    response.StorageClass = ParseStorageClass(xmlReader.ReadString());
                }
            }

            return response;
        }


        public GetBucketMetadataResponse ParseGetBucketMetadataResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketMetadataResponse();

            string storageClass;
            httpResponse.Headers.TryGetValue(iheaders.DefaultStorageClassHeader(), out storageClass);

            response.StorageClass = ParseStorageClass(storageClass);

            if (httpResponse.Headers.ContainsKey(iheaders.ServerVersionHeader()))
            {
                response.ObsVersion = httpResponse.Headers[iheaders.ServerVersionHeader()];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.BucketRegionHeader()))
            {
                response.Location = httpResponse.Headers[iheaders.BucketRegionHeader()];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.AzRedundancyHeader()))
            {
                var value = httpResponse.Headers[iheaders.AzRedundancyHeader()];
                if (Constants.ThreeAz.Equals(value))
                {
                    response.AvailableZone = AvailableZoneEnum.MultiAz;
                }
            }

            return response;
        }


        public GetBucketLocationResponse ParseGetBucketLocationResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketLocationResponse();

            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if (xmlReader.Name.Equals(BucketLocationTag))
                {
                    response.Location = xmlReader.ReadString();
                }
            }

            return response;
        }



        public GetBucketStorageInfoResponse ParseGetBucketStorageInfoResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketStorageInfoResponse();

            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("Size".Equals(xmlReader.Name))
                {
                    response.Size = Convert.ToInt64(xmlReader.ReadString());
                }
                else if ("ObjectNumber".Equals(xmlReader.Name))
                {
                    response.ObjectNumber = Convert.ToInt64(xmlReader.ReadString());
                }
            }

            return response;
        }


        public ListObjectsResponse ParseListObjectsResponse(HttpResponse httpResponse)
        {
            var response = new ListObjectsResponse();

            if (httpResponse.Headers.ContainsKey(iheaders.BucketRegionHeader()))
            {
                response.Location = httpResponse.Headers[iheaders.BucketRegionHeader()];
            }

            using var xmlReader           = XmlReader.Create(httpResponse.Content);
            ObsObject currentObject       = null;
            var       innerCommonprefixes = false;

            while (xmlReader.Read())
            {
                if ("Name".Equals(xmlReader.Name))
                {
                    response.BucketName = xmlReader.ReadString();
                }
                else if ("Prefix".Equals(xmlReader.Name))
                {
                    if (innerCommonprefixes)
                    {
                        response.CommonPrefixes.Add(xmlReader.ReadString());
                    }
                    else
                    {
                        response.Prefix = xmlReader.ReadString();
                    }
                }
                else if ("Marker".Equals(xmlReader.Name))
                {
                    response.Marker = xmlReader.ReadString();
                }
                else if ("NextMarker".Equals(xmlReader.Name))
                {
                    response.NextMarker = xmlReader.ReadString();
                }
                else if ("MaxKeys".Equals(xmlReader.Name))
                {
                    response.MaxKeys = CommonUtil.ParseToInt32(xmlReader.ReadString());
                }
                else if ("Delimiter".Equals(xmlReader.Name))
                {
                    response.Delimiter = xmlReader.ReadString();
                }
                else if ("IsTruncated".Equals(xmlReader.Name))
                {
                    response.IsTruncated = Convert.ToBoolean(xmlReader.ReadString());
                }
                else if ("Contents".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObject = new ObsObject();
                        response.ObsObjects.Add(currentObject);
                    }
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    currentObject.ObjectKey = xmlReader.ReadString();
                }
                else if ("LastModified".Equals(xmlReader.Name))
                {
                    currentObject.LastModified = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }
                else if ("ETag".Equals(xmlReader.Name))
                {
                    currentObject.ETag = xmlReader.ReadString();
                }
                else if ("Size".Equals(xmlReader.Name))
                {
                    currentObject.Size = Convert.ToInt64(xmlReader.ReadString());
                }
                else if ("Type".Equals(xmlReader.Name))
                {
                    currentObject.Appendable = "Appendable".Equals(xmlReader.ReadString());
                }
                else if ("Owner".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObject.Owner = new Owner();
                    }
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    currentObject.Owner.Id = xmlReader.ReadString();
                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {
                    currentObject.Owner.DisplayName = xmlReader.ReadString();
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    currentObject.StorageClass = ParseStorageClass(xmlReader.ReadString());
                }
                else if ("CommonPrefixes".Equals(xmlReader.Name))
                {
                    innerCommonprefixes = xmlReader.IsStartElement();
                }
            }

            return response;
        }

        public ListVersionsResponse ParseListVersionsResponse(HttpResponse httpResponse)
        {
            var response = new ListVersionsResponse();

            if (httpResponse.Headers.ContainsKey(iheaders.BucketRegionHeader()))
            {
                response.Location = httpResponse.Headers[iheaders.BucketRegionHeader()];
            }

            using var        xmlReader           = XmlReader.Create(httpResponse.Content);
            ObsObjectVersion currentObject       = null;
            var              innerCommonprefixes = false;

            while (xmlReader.Read())
            {
                if ("Name".Equals(xmlReader.Name))
                {
                    response.BucketName = xmlReader.ReadString();
                }
                else if ("Prefix".Equals(xmlReader.Name))
                {
                    if (innerCommonprefixes)
                    {
                        response.CommonPrefixes.Add(xmlReader.ReadString());
                    }
                    else
                    {
                        response.Prefix = xmlReader.ReadString();
                    }
                }
                else if ("KeyMarker".Equals(xmlReader.Name))
                {
                    response.KeyMarker = xmlReader.ReadString();
                }
                else if ("NextKeyMarker".Equals(xmlReader.Name))
                {
                    response.NextKeyMarker = xmlReader.ReadString();
                }
                else if ("VersionIdMarker".Equals(xmlReader.Name))
                {
                    response.VersionIdMarker = xmlReader.ReadString();
                }
                else if ("NextVersionIdMarker".Equals(xmlReader.Name))
                {
                    response.NextVersionIdMarker = xmlReader.ReadString();
                }
                else if ("MaxKeys".Equals(xmlReader.Name))
                {
                    response.MaxKeys = CommonUtil.ParseToInt32(xmlReader.ReadString());
                }
                else if ("Delimiter".Equals(xmlReader.Name))
                {
                    response.Delimiter = xmlReader.ReadString();
                }
                else if ("IsTruncated".Equals(xmlReader.Name))
                {
                    response.IsTruncated = Convert.ToBoolean(xmlReader.ReadString());
                }
                else if ("Version".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObject = new ObsObjectVersion();
                        response.Versions.Add(currentObject);
                    }
                }
                else if ("DeleteMarker".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObject                = new ObsObjectVersion
                        {
                            IsDeleteMarker = true
                        };
                        response.Versions.Add(currentObject);
                    }
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    currentObject.ObjectKey = xmlReader.ReadString();
                }
                else if ("VersionId".Equals(xmlReader.Name))
                {
                    currentObject.VersionId = xmlReader.ReadString();
                }
                else if ("LastModified".Equals(xmlReader.Name))
                {
                    currentObject.LastModified = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }
                else if ("IsLatest".Equals(xmlReader.Name))
                {
                    currentObject.IsLatest = Convert.ToBoolean(xmlReader.ReadString());
                }
                else if ("ETag".Equals(xmlReader.Name))
                {
                    currentObject.ETag = xmlReader.ReadString();
                }
                else if ("Size".Equals(xmlReader.Name))
                {
                    currentObject.Size = Convert.ToInt64(xmlReader.ReadString());
                }
                else if ("Type".Equals(xmlReader.Name))
                {
                    currentObject.Appendable = "Appendable".Equals(xmlReader.ReadString());
                }
                else if ("Owner".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObject.Owner = new Owner();
                    }
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    currentObject.Owner.Id = xmlReader.ReadString();
                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {
                    currentObject.Owner.DisplayName = xmlReader.ReadString();
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    currentObject.StorageClass = ParseStorageClass(xmlReader.ReadString());
                }
                else if ("CommonPrefixes".Equals(xmlReader.Name))
                {
                    innerCommonprefixes = xmlReader.IsStartElement();
                }
            }

            return response;
        }

        public GetBucketQuotaResponse ParseGetBucketQuotaResponse(HttpResponse httpResponse)
        {
            var       response  = new GetBucketQuotaResponse();
            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("StorageQuota".Equals(xmlReader.Name))
                {
                    response.StorageQuota = Convert.ToInt64(xmlReader.ReadString());
                }
            }

            return response;
        }

        public virtual GetBucketAclResponse ParseGetBucketAclResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketAclResponse();
            response.AccessControlList = ParseAccessControlList(httpResponse);
            return response;
        }

        public ListMultipartUploadsResponse ParseListMultipartUploadsResponse(HttpResponse httpResponse)
        {
            var response = new ListMultipartUploadsResponse();

            using var       xmlReader     = XmlReader.Create(httpResponse.Content);
            MultipartUpload currentUpload = null;

            var innerOwner     = false;
            var innerInitiator = false;

            while (xmlReader.Read())
            {
                if ("Bucket".Equals(xmlReader.Name))
                {
                    response.BucketName = xmlReader.ReadString();
                }

                else if ("KeyMarker".Equals(xmlReader.Name))
                {
                    response.KeyMarker = xmlReader.ReadString();
                }
                else if ("NextKeyMarker".Equals(xmlReader.Name))
                {
                    response.NextKeyMarker = xmlReader.ReadString();
                }
                else if ("UploadIdMarker".Equals(xmlReader.Name))
                {
                    response.UploadIdMarker = xmlReader.ReadString();
                }
                else if ("NextUploadIdMarker".Equals(xmlReader.Name))
                {
                    response.NextUploadIdMarker = xmlReader.ReadString();
                }
                else if ("MaxUploads".Equals(xmlReader.Name))
                {
                    response.MaxUploads = CommonUtil.ParseToInt32(xmlReader.ReadString());
                }
                else if ("Delimiter".Equals(xmlReader.Name))
                {
                    response.Delimiter = xmlReader.ReadString();
                }
                else if ("IsTruncated".Equals(xmlReader.Name))
                {
                    response.IsTruncated = Convert.ToBoolean(xmlReader.ReadString());
                }
                else if ("Upload".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentUpload = new MultipartUpload();
                        response.MultipartUploads.Add(currentUpload);
                    }
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    currentUpload.ObjectKey = xmlReader.ReadString();
                }
                else if ("UploadId".Equals(xmlReader.Name))
                {
                    currentUpload.UploadId = xmlReader.ReadString();
                }
                else if ("Initiated".Equals(xmlReader.Name))
                {
                    currentUpload.Initiated = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }
                else if ("Initiator".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentUpload.Initiator = new Initiator();
                    }
                    innerInitiator = xmlReader.IsStartElement();
                }
                else if ("Owner".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentUpload.Owner = new Owner();
                    }
                    innerOwner = xmlReader.IsStartElement();
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    if (innerInitiator)
                    {
                        currentUpload.Initiator.Id = xmlReader.ReadString();
                    }
                    else if (innerOwner)
                    {
                        currentUpload.Owner.Id = xmlReader.ReadString();
                    }
                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {
                    if (innerInitiator)
                    {
                        currentUpload.Initiator.DisplayName = xmlReader.ReadString();
                    }
                    else if (innerOwner)
                    {
                        currentUpload.Owner.DisplayName = xmlReader.ReadString();
                    }
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    currentUpload.StorageClass = ParseStorageClass(xmlReader.ReadString());
                }
                else if ("Prefix".Equals(xmlReader.Name))
                {
                    response.CommonPrefixes.Add(xmlReader.ReadString());
                }
            }

            return response;
        }

        public virtual GetBucketLoggingResponse ParseGetBucketLoggingResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketLoggingResponse();

            using var xmlReader    = XmlReader.Create(httpResponse.Content);
            Grant     currentGrant = null;
            while (xmlReader.Read())
            {
                if ("BucketLoggingStatus".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new LoggingConfiguration();
                    }
                }
                else if ("TargetBucket".Equals(xmlReader.Name))
                {
                    response.Configuration.TargetBucketName = xmlReader.ReadString();
                }
                else if ("TargetPrefix".Equals(xmlReader.Name))
                {
                    response.Configuration.TargetPrefix = xmlReader.ReadString();
                }
                else if ("Grant".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentGrant = new Grant();
                        response.Configuration.Grants.Add(currentGrant);
                    }
                }
                else if ("Grantee".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        Grantee grantee;
                        if (xmlReader.GetAttribute("xsi:type").Equals("CanonicalUser"))
                        {
                            grantee = new CanonicalGrantee();
                        }
                        else
                        {
                            grantee = new GroupGrantee();
                        }
                        currentGrant.Grantee = grantee;
                    }
                }
                else if ("ID".Equals(xmlReader.Name))
                {

                    var grantee = currentGrant.Grantee as CanonicalGrantee;
                    if (grantee != null)
                    {
                        grantee.Id = xmlReader.ReadString();
                    }

                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {

                    var grantee = currentGrant.Grantee as CanonicalGrantee;
                    if (grantee != null)
                    {
                        grantee.DisplayName = xmlReader.ReadString();
                    }
                }
                else if ("URI".Equals(xmlReader.Name))
                {
                    var grantee = currentGrant.Grantee as GroupGrantee;
                    if (grantee != null)
                    {
                        grantee.GroupGranteeType = ParseGroupGrantee(xmlReader.ReadString());
                    }
                }
                else if ("Permission".Equals(xmlReader.Name))
                {
                    currentGrant.Permission = ParsePermission(xmlReader.ReadString());
                }
            }

            return response;
        }

        public GetBucketPolicyResponse ParseGetBucketPolicyResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketPolicyResponse();

            using var streamReader = new StreamReader(httpResponse.Content, Encoding.UTF8);
            response.Policy = streamReader.ReadToEnd();
            return response;
        }

        public GetBucketCorsResponse ParseGetBucketCorsResponse(HttpResponse httpResponse)
        {
            var       response        = new GetBucketCorsResponse();
            using var xmlReader       = XmlReader.Create(httpResponse.Content);
            CorsRule  currentCorsRule = null;
            while (xmlReader.Read())
            {
                if ("CORSConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new CorsConfiguration();
                    }
                }
                else if ("CORSRule".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentCorsRule = new CorsRule();
                        response.Configuration.Rules.Add(currentCorsRule);
                    }
                }
                else if ("AllowedMethod".Equals(xmlReader.Name))
                {
                    var temp = ParseHttpVerb(xmlReader.ReadString());
                    if (temp.HasValue)
                    {
                        currentCorsRule.AllowedMethods.Add(temp.Value);
                    }
                }
                else if ("AllowedOrigin".Equals(xmlReader.Name))
                {
                    currentCorsRule.AllowedOrigins.Add(xmlReader.ReadString());
                }
                else if ("AllowedHeader".Equals(xmlReader.Name))
                {
                    currentCorsRule.AllowedHeaders.Add(xmlReader.ReadString());
                }
                else if ("MaxAgeSeconds".Equals(xmlReader.Name))
                {
                    currentCorsRule.MaxAgeSeconds = CommonUtil.ParseToInt32(xmlReader.ReadString());
                }
                else if ("ExposeHeader".Equals(xmlReader.Name))
                {
                    currentCorsRule.ExposeHeaders.Add(xmlReader.ReadString());
                }
            }

            return response;
        }

        public GetBucketLifecycleResponse ParseGetBucketLifecycleResponse(HttpResponse httpResponse)
        {
            var                         response                           = new GetBucketLifecycleResponse();
            using var                   xmlReader                          = XmlReader.Create(httpResponse.Content);
            LifecycleRule               currentRule                        = null;
            var                         innerExpiration                    = false;
            var                         innerNoncurrentVersionExpiration   = false;
            var                         innerTransition                    = false;
            var                         innerNoncurrentVersionTransition   = false;
            Transition                  currentTransition                  = null;
            NoncurrentVersionTransition currentNoncurrentVersionTransition = null;
            while (xmlReader.Read())
            {
                if ("LifecycleConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new LifecycleConfiguration();
                    }
                }
                else if ("Rule".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRule = new LifecycleRule();
                        response.Configuration.Rules.Add(currentRule);
                    }
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    currentRule.Id = xmlReader.ReadString();
                }
                else if ("Prefix".Equals(xmlReader.Name))
                {
                    currentRule.Prefix = xmlReader.ReadString();
                }
                else if ("Status".Equals(xmlReader.Name))
                {
                    var temp = ParseRuleStatus(xmlReader.ReadString());
                    if (temp.HasValue)
                    {
                        currentRule.Status = temp.Value;
                    }
                }
                else if ("Expiration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRule.Expiration = new Expiration();
                    }
                    innerExpiration = xmlReader.IsStartElement();
                }
                else if ("NoncurrentVersionExpiration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRule.NoncurrentVersionExpiration = new NoncurrentVersionExpiration();
                    }
                    innerNoncurrentVersionExpiration = xmlReader.IsStartElement();
                }
                else if ("Transition".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentTransition = new Transition();
                        currentRule.Transitions.Add(currentTransition);
                    }

                    innerTransition = xmlReader.IsStartElement();
                }
                else if ("NoncurrentVersionTransition".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentNoncurrentVersionTransition = new NoncurrentVersionTransition();
                        currentRule.NoncurrentVersionTransitions.Add(currentNoncurrentVersionTransition);
                    }
                    innerNoncurrentVersionTransition = xmlReader.IsStartElement();
                }
                else if ("Days".Equals(xmlReader.Name))
                {
                    if (innerExpiration)
                    {
                        currentRule.Expiration.Days = CommonUtil.ParseToInt32(xmlReader.ReadString());
                    }
                    else if (innerTransition)
                    {
                        currentTransition.Days = CommonUtil.ParseToInt32(xmlReader.ReadString());
                    }
                }
                else if ("Date".Equals(xmlReader.Name))
                {
                    if (innerExpiration)
                    {
                        currentRule.Expiration.Date = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                    }
                    else if (innerTransition)
                    {
                        currentTransition.Date = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                    }
                }
                else if ("NoncurrentDays".Equals(xmlReader.Name))
                {
                    if (innerNoncurrentVersionExpiration)
                    {
                        currentRule.NoncurrentVersionExpiration.NoncurrentDays = Convert.ToInt32(xmlReader.ReadString());
                    }
                    else if (innerNoncurrentVersionTransition)
                    {
                        currentNoncurrentVersionTransition.NoncurrentDays = Convert.ToInt32(xmlReader.ReadString());
                    }
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    if (innerTransition)
                    {
                        currentTransition.StorageClass = ParseStorageClass(xmlReader.ReadString());
                    }
                    else if (innerNoncurrentVersionTransition)
                    {
                        currentNoncurrentVersionTransition.StorageClass = ParseStorageClass(xmlReader.ReadString());
                    }
                }

            }

            return response;
        }

        public GetBucketWebsiteResponse ParseGetBucketWebsiteResponse(HttpResponse httpResponse)
        {
            var response = new GetBucketWebsiteResponse();

            using var   xmlReader                  = XmlReader.Create(httpResponse.Content);
            var         innerRedirectAllRequestsTo = false;
            RoutingRule currentRoutingRule         = null;
            while (xmlReader.Read())
            {
                if ("WebsiteConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new WebsiteConfiguration();
                    }
                }
                else if ("RedirectAllRequestsTo".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration.RedirectAllRequestsTo = new RedirectBasic();
                    }

                    innerRedirectAllRequestsTo = xmlReader.IsStartElement();
                }
                else if ("HostName".Equals(xmlReader.Name))
                {
                    if (innerRedirectAllRequestsTo)
                    {
                        response.Configuration.RedirectAllRequestsTo.HostName = xmlReader.ReadString();
                    }
                    else
                    {
                        currentRoutingRule.Redirect.HostName = xmlReader.ReadString();
                    }
                }
                else if ("Protocol".Equals(xmlReader.Name))
                {
                    if (innerRedirectAllRequestsTo)
                    {
                        response.Configuration.RedirectAllRequestsTo.Protocol = ParseProtocol(xmlReader.ReadString());
                    }
                    else
                    {
                        currentRoutingRule.Redirect.Protocol = ParseProtocol(xmlReader.ReadString());
                    }
                }
                else if ("Suffix".Equals(xmlReader.Name))
                {
                    response.Configuration.IndexDocument = xmlReader.ReadString();
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    response.Configuration.ErrorDocument = xmlReader.ReadString();
                }
                else if ("RoutingRule".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRoutingRule = new RoutingRule();
                        response.Configuration.RoutingRules.Add(currentRoutingRule);
                    }
                }
                else if ("Condition".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRoutingRule.Condition = new Condition();
                    }
                }
                else if ("KeyPrefixEquals".Equals(xmlReader.Name))
                {
                    currentRoutingRule.Condition.KeyPrefixEquals = xmlReader.ReadString();
                }
                else if ("HttpErrorCodeReturnedEquals".Equals(xmlReader.Name))
                {
                    currentRoutingRule.Condition.HttpErrorCodeReturnedEquals = xmlReader.ReadString();
                }
                else if ("Redirect".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRoutingRule.Redirect = new Redirect();
                    }
                }
                else if ("ReplaceKeyPrefixWith".Equals(xmlReader.Name))
                {
                    currentRoutingRule.Redirect.ReplaceKeyPrefixWith = xmlReader.ReadString();
                }
                else if ("ReplaceKeyWith".Equals(xmlReader.Name))
                {
                    currentRoutingRule.Redirect.ReplaceKeyWith = xmlReader.ReadString();
                }
                else if ("HttpRedirectCode".Equals(xmlReader.Name))
                {
                    currentRoutingRule.Redirect.HttpRedirectCode = xmlReader.ReadString();
                }
            }

            return response;
        }

        public GetBucketVersioningResponse ParseGetBucketVersioningResponse(HttpResponse httpResponse)
        {
            var       response  = new GetBucketVersioningResponse();
            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("VersioningConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new VersioningConfiguration();
                    }
                }
                else if ("Status".Equals(xmlReader.Name))
                {
                    response.Configuration.Status = ParseVersionStatusEnum(xmlReader.ReadString());
                }
            }

            return response;
        }

        public GetBucketTaggingResponse ParseGetBucketTaggingResponse(HttpResponse httpResponse)
        {
            var       response   = new GetBucketTaggingResponse();
            using var xmlReader  = XmlReader.Create(httpResponse.Content);
            Tag       currentTag = null;
            while (xmlReader.Read())
            {

                if ("Tag".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentTag = new Tag();
                        response.Tags.Add(currentTag);
                    }
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    currentTag.Key = xmlReader.ReadString();
                }
                else if ("Value".Equals(xmlReader.Name))
                {
                    currentTag.Value = xmlReader.ReadString();
                }
            }

            return response;
        }

        public GetBucketNotificationReponse ParseGetBucketNotificationReponse(HttpResponse httpResponse)
        {
            var                        response                        = new GetBucketNotificationReponse();
            using var                  xmlReader                       = XmlReader.Create(httpResponse.Content);
            TopicConfiguration         currentTc                       = null;
            FunctionGraphConfiguration currentFc                       = null;
            var                        innerTopicConfiguration         = false;
            var                        innerFunctionGraphConfiguration = false;
            FilterRule                 currentFr                       = null;
            while (xmlReader.Read())
            {
                if ("NotificationConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new NotificationConfiguration();
                    }
                }
                else if ("TopicConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentTc = new TopicConfiguration();
                        response.Configuration.TopicConfigurations.Add(currentTc);
                    }
                    innerTopicConfiguration = xmlReader.IsStartElement();
                }
                else if ("FunctionGraphConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentFc = new FunctionGraphConfiguration();
                        response.Configuration.FunctionGraphConfigurations.Add(currentFc);
                    }
                    innerFunctionGraphConfiguration = xmlReader.IsStartElement();
                }
                else if ("Id".Equals(xmlReader.Name))
                {
                    if (innerTopicConfiguration)
                    {
                        currentTc.Id = xmlReader.ReadString();
                    }else if (innerFunctionGraphConfiguration)
                    {
                        currentFc.Id = xmlReader.ReadString(); 
                    }
                }
                else if ("FilterRule".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        if (innerTopicConfiguration)
                        {
                            currentFr = new FilterRule();
                            currentTc.FilterRules.Add(currentFr);
                        }else if (innerFunctionGraphConfiguration)
                        {
                            currentFc.FilterRules.Add(currentFr);
                        }
                    }

                }
                else if ("Name".Equals(xmlReader.Name))
                {
                    currentFr.Name = ParseFilterName(xmlReader.ReadString());
                }
                else if ("Value".Equals(xmlReader.Value))
                {
                    currentFr.Value = xmlReader.ReadString();
                }
                else if ("Topic".Equals(xmlReader.Name))
                {
                    currentTc.Topic = xmlReader.ReadString();
                }
                else if ("FunctionGraph".Equals(xmlReader.Name))
                {
                    currentFc.FunctionGraph = xmlReader.ReadString();
                }
                else if ("Event".Equals(xmlReader.Name))
                {
                    if (innerTopicConfiguration)
                    {
                        var temp = ParseEventTypeEnum(xmlReader.ReadString());
                        if (temp.HasValue)
                        {
                            currentTc.Events.Add(temp.Value);
                        }
                    }else if (innerFunctionGraphConfiguration)
                    {
                        var temp = ParseEventTypeEnum(xmlReader.ReadString());
                        if (temp.HasValue)
                        {
                            currentFc.Events.Add(temp.Value);
                        }
                    }
                }
            }

            return response;
        }

        public DeleteObjectResponse ParseDeleteObjectResponse(HttpResponse httpResponse)
        {
            var response = new DeleteObjectResponse();

            if (httpResponse.Headers.ContainsKey(iheaders.VersionIdHeader()))
            {
                response.VersionId = httpResponse.Headers[iheaders.VersionIdHeader()];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.DeleteMarkerHeader()))
            {
                response.DeleteMarker = Convert.ToBoolean(httpResponse.Headers[iheaders.DeleteMarkerHeader()]);
            }
            return response;
        }

        public DeleteObjectsResponse ParseDeleteObjectsResponse(HttpResponse httpResponse)
        {
            var           response     = new DeleteObjectsResponse();
            using var     xmlReader    = XmlReader.Create(httpResponse.Content);
            DeletedObject currentObj   = null;
            DeleteError   currentErr   = null;
            var           innerDeleted = false;
            var           innerError   = false;
            while (xmlReader.Read())
            {
                if ("Deleted".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentObj = new DeletedObject();
                        response.DeletedObjects.Add(currentObj);
                    }
                    innerDeleted = xmlReader.IsStartElement();
                }
                else if ("Error".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentErr = new DeleteError();
                        response.DeleteErrors.Add(currentErr);
                    }
                    innerError = xmlReader.IsStartElement();
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    if (innerDeleted)
                    {
                        currentObj.ObjectKey = xmlReader.ReadString();
                    }
                    else if (innerError)
                    {
                        currentErr.ObjectKey = xmlReader.ReadString();
                    }

                }else if ("VersionId".Equals(xmlReader.Name))
                {
                    if (innerDeleted)
                    {
                        currentObj.VersionId = xmlReader.ReadString();
                    }
                    else if (innerError)
                    {
                        currentErr.VersionId = xmlReader.ReadString();
                    }
                }
                else if ("Code".Equals(xmlReader.Name))
                {
                    currentErr.Code = xmlReader.ReadString();
                }else if ("Message".Equals(xmlReader.Name))
                {
                    currentErr.Message = xmlReader.ReadString();
                }else if ("DeleteMarker".Equals(xmlReader.Name))
                {
                    currentObj.DeleteMarker = Convert.ToBoolean(xmlReader.ReadString());
                }else if ("DeleteMarkerVersionId".Equals(xmlReader.Name))
                {
                    currentObj.DeleteMarkerVersionId = xmlReader.ReadString();
                }
            }

            return response;
        }

        public ListPartsResponse ParseListPartsResponse(HttpResponse httpResponse)
        {
            var        response       = new ListPartsResponse();
            using var  xmlReader      = XmlReader.Create(httpResponse.Content);
            PartDetail currentPart    = null;
            var        innerInitiator = false;
            var        innerOwner     = false;
            while (xmlReader.Read())
            {
                if ("Bucket".Equals(xmlReader.Name))
                {
                    response.BucketName = xmlReader.ReadString();
                } else if ("Key".Equals(xmlReader.Name))
                {
                    response.ObjectKey = xmlReader.ReadString();
                } else if ("UploadId".Equals(xmlReader.Name))
                {
                    response.UploadId = xmlReader.ReadString();
                }else if ("Initiator".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Initiator = new Initiator();
                    }
                    innerInitiator = xmlReader.IsStartElement();
                }
                else if ("Owner".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Owner = new Owner();
                    }
                    innerOwner = xmlReader.IsStartElement();
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    if (innerInitiator)
                    {
                        response.Initiator.Id = xmlReader.ReadString();
                    }else if (innerOwner)
                    {
                        response.Owner.Id = xmlReader.ReadString();
                    }
                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {
                    if (innerInitiator)
                    {
                        response.Initiator.DisplayName = xmlReader.ReadString();
                    }
                    else if (innerOwner)
                    {
                        response.Owner.DisplayName = xmlReader.ReadString();
                    }
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    response.StorageClass = ParseStorageClass(xmlReader.ReadString());
                } else if ("PartNumberMarker".Equals(xmlReader.Name))
                {
                    response.PartNumberMarker = CommonUtil.ParseToInt32(xmlReader.ReadString());
                } else if ("NextPartNumberMarker".Equals(xmlReader.Name))
                {
                    response.NextPartNumberMarker = CommonUtil.ParseToInt32(xmlReader.ReadString());
                } else if ("MaxParts".Equals(xmlReader.Name))
                {
                    response.MaxParts = CommonUtil.ParseToInt32(xmlReader.ReadString());
                } else if ("IsTruncated".Equals(xmlReader.Name))
                {
                    response.IsTruncated = Convert.ToBoolean(xmlReader.ReadString());
                } else if ("Part".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentPart = new PartDetail();
                        response.Parts.Add(currentPart);
                    }
                } else if ("PartNumber".Equals(xmlReader.Name))
                {
                    currentPart.PartNumber = Convert.ToInt32(xmlReader.ReadString());
                }else if ("LastModified".Equals(xmlReader.Name))
                {
                    currentPart.LastModified = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }else if ("ETag".Equals(xmlReader.Name))
                {
                    currentPart.ETag = xmlReader.ReadString();
                }else if ("Size".Equals(xmlReader.Name))
                {
                    currentPart.Size = Convert.ToInt64(xmlReader.ReadString());
                }
            }

            return response;
        }

        public CompleteMultipartUploadResponse ParseCompleteMultipartUploadResponse(HttpResponse httpResponse)
        {
            var response = new CompleteMultipartUploadResponse();
            using (var xmlReader = XmlReader.Create(httpResponse.Content))
            {
                while (xmlReader.Read())
                {
                    if ("Location".Equals(xmlReader.Name))
                    {
                        response.Location = xmlReader.ReadString();
                    }else if ("Bucket".Equals(xmlReader.Name))
                    {
                        response.BucketName = xmlReader.ReadString();
                    }else if ("Key".Equals(xmlReader.Name))
                    {
                        response.ObjectKey = xmlReader.ReadString();
                    }else if ("ETag".Equals(xmlReader.Name))
                    {
                        response.ETag = xmlReader.ReadString();
                    }
                }
            }

            if (httpResponse.Headers.ContainsKey(iheaders.VersionIdHeader()))
            {
                response.VersionId = httpResponse.Headers[iheaders.VersionIdHeader()];
            }

            response.ObjectUrl = httpResponse.RequestUrl;

            return response;
        }

        private AccessControlList ParseAccessControlList(HttpResponse httpResponse)
        {
            using var xmlReader    = XmlReader.Create(httpResponse.Content);
            var       acl          = new AccessControlList();
            var       innerOwner   = false;
            Grant     currentGrant = null;
            while (xmlReader.Read())
            {
                if ("Owner".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        acl.Owner = new Owner();
                    }
                    innerOwner = xmlReader.IsStartElement();
                }
                else if ("ID".Equals(xmlReader.Name))
                {
                    if (innerOwner)
                    {
                        acl.Owner.Id = xmlReader.ReadString();
                    }
                    else
                    {
                        var grantee = currentGrant.Grantee as CanonicalGrantee;
                        if (grantee != null)
                        {
                            grantee.Id = xmlReader.ReadString();
                        }
                    }
                }
                else if ("DisplayName".Equals(xmlReader.Name))
                {
                    if (innerOwner)
                    {
                        acl.Owner.DisplayName = xmlReader.ReadString();
                    }
                    else
                    {
                        var grantee = currentGrant.Grantee as CanonicalGrantee;
                        if (grantee != null)
                        {
                            grantee.DisplayName = xmlReader.ReadString();
                        }
                    }
                }
                else if ("Grant".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentGrant = new Grant();
                        acl.Grants.Add(currentGrant);
                    }
                }
                else if ("Grantee".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        Grantee grantee;
                        if (xmlReader.GetAttribute("xsi:type").Equals("CanonicalUser"))
                        {
                            grantee = new CanonicalGrantee();
                        }
                        else
                        {
                            grantee = new GroupGrantee();
                        }
                        currentGrant.Grantee = grantee;
                    }
                }
                else if ("URI".Equals(xmlReader.Name))
                {
                    var grantee = currentGrant.Grantee as GroupGrantee;
                    if (grantee != null)
                    {
                        grantee.GroupGranteeType = ParseGroupGrantee(xmlReader.ReadString());
                    }
                }
                else if ("Permission".Equals(xmlReader.Name))
                {
                    currentGrant.Permission = ParsePermission(xmlReader.ReadString());
                }
            }
            return acl;
        }

        public virtual GetObjectAclResponse ParseGetObjectAclResponse(HttpResponse httpResponse)
        {
            var response = new GetObjectAclResponse();
            response.AccessControlList = ParseAccessControlList(httpResponse);
            return response;
        }

        public PutObjectResponse ParsePutObjectResponse(HttpResponse httpResponse)
        {
            var response = new PutObjectResponse();

            if (httpResponse.Headers.ContainsKey(iheaders.VersionIdHeader()))
            {
                response.VersionId = httpResponse.Headers[iheaders.VersionIdHeader()];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.StorageClassHeader()))
            {
                response.StorageClass = ParseStorageClass(httpResponse.Headers[iheaders.StorageClassHeader()]);
            }

            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ETag))
            {
                response.ETag = httpResponse.Headers[Constants.CommonHeaders.ETag];
            }

            response.ObjectUrl = httpResponse.RequestUrl;

            return response;
        }

        public CopyObjectResponse ParseCopyObjectResponse(HttpResponse httpResponse)
        {
            var response = new CopyObjectResponse();
            if (httpResponse.Headers.ContainsKey(iheaders.VersionIdHeader()))
            {
                response.VersionId = httpResponse.Headers[iheaders.VersionIdHeader()];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.StorageClassHeader()))
            {
                response.StorageClass = ParseStorageClass(httpResponse.Headers[iheaders.StorageClassHeader()]);
            }

            if (httpResponse.Headers.ContainsKey(iheaders.CopySourceVersionIdHeader()))
            {
                response.SourceVersionId = httpResponse.Headers[iheaders.CopySourceVersionIdHeader()];
            }

            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("LastModified".Equals(xmlReader.Name))
                {
                    response.LastModified = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }else if ("ETag".Equals(xmlReader.Name))
                {
                    response.ETag = xmlReader.ReadString();
                }
            }

            return response;
        }

        public InitiateMultipartUploadResponse ParseInitiateMultipartUploadResponse(HttpResponse httpResponse)
        {
            var       response  = new InitiateMultipartUploadResponse();
            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("Bucket".Equals(xmlReader.Name))
                {
                    response.BucketName = xmlReader.ReadString(); 
                }
                else if ("Key".Equals(xmlReader.Name))
                {
                    response.ObjectKey = xmlReader.ReadString();
                }
                else if ("UploadId".Equals(xmlReader.Name))
                {
                    response.UploadId = xmlReader.ReadString();
                }
            }

            return response;
        }

        public CopyPartResponse ParseCopyPartResponse(HttpResponse httpResponse)
        {
            var       response  = new CopyPartResponse();
            using var xmlReader = XmlReader.Create(httpResponse.Content);
            while (xmlReader.Read())
            {
                if ("LastModified".Equals(xmlReader.Name))
                {
                    response.LastModified = CommonUtil.ParseToDateTime(xmlReader.ReadString());
                }
                else if ("ETag".Equals(xmlReader.Name))
                {
                    response.ETag = xmlReader.ReadString();
                }
            }

            return response;
        }

        public UploadPartResponse ParseUploadPartResponse(HttpResponse httpResponse)
        {
            var response = new UploadPartResponse();
            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ETag))
            {
                response.ETag = httpResponse.Headers[Constants.CommonHeaders.ETag];
            }
            return response;
        }

        public GetBucketReplicationResponse ParseGetBucketReplicationResponse(HttpResponse httpResponse)
        {
            var             response    = new GetBucketReplicationResponse();
            using var       xmlReader   = XmlReader.Create(httpResponse.Content);
            ReplicationRule currentRule = null;
            while (xmlReader.Read())
            {
                if ("ReplicationConfiguration".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        response.Configuration = new ReplicationConfiguration();
                    }
                }
                else if ("Agency".Equals(xmlReader.Name))
                {
                    response.Configuration.Agency = xmlReader.ReadString();
                }
                else if ("Rule".Equals(xmlReader.Name))
                {
                    if (xmlReader.IsStartElement())
                    {
                        currentRule = new ReplicationRule();
                        response.Configuration.Rules.Add(currentRule);
                    }
                }else if ("ID".Equals(xmlReader.Name))
                {
                    currentRule.Id = xmlReader.ReadString();
                }
                else if ("Prefix".Equals(xmlReader.Name))
                {
                    currentRule.Prefix = xmlReader.ReadString();
                }
                else if ("Status".Equals(xmlReader.Name))
                {
                    var temp = ParseRuleStatus(xmlReader.ReadString());
                    if (temp.HasValue)
                    {
                        currentRule.Status = temp.Value;
                    }
                }
                else if ("Bucket".Equals(xmlReader.Name))
                {
                    currentRule.TargetBucketName = xmlReader.ReadString();
                }
                else if ("StorageClass".Equals(xmlReader.Name))
                {
                    currentRule.TargetStorageClass = ParseStorageClass(xmlReader.ReadString());
                }
            }

            return response;
        }

        protected void ParseGetObjectMetadataResponse(HttpResponse httpResponse, GetObjectMetadataResponse response)
        {
            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ETag))
            {
                response.ETag = httpResponse.Headers[Constants.CommonHeaders.ETag];
            }

            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ContentLength))
            {
                response.ContentLength = Convert.ToInt64(httpResponse.Headers[Constants.CommonHeaders.ContentLength]);
            }

            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ContentType))
            {
                response.ContentType = httpResponse.Headers[Constants.CommonHeaders.ContentType];
            }
            if (httpResponse.Headers.ContainsKey(iheaders.VersionIdHeader()))
            {
                response.VersionId = httpResponse.Headers[iheaders.VersionIdHeader()];
            }
            if (httpResponse.Headers.ContainsKey(iheaders.WebsiteRedirectLocationHeader()))
            {
                response.WebsiteRedirectLocation = httpResponse.Headers[iheaders.WebsiteRedirectLocationHeader()];
            }

            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.LastModified))
            {
                response.LastModified = CommonUtil.ParseToDateTime(httpResponse.Headers[Constants.CommonHeaders.LastModified], Constants.RFC822DateFormat, Constants.ISO8601DateFormat);
            }

            if (httpResponse.Headers.ContainsKey(iheaders.StorageClassHeader()))
            {
                response.StorageClass = ParseStorageClass(httpResponse.Headers[iheaders.StorageClassHeader()]);
            }
            if (httpResponse.Headers.ContainsKey(iheaders.DeleteMarkerHeader()))
            {
                response.DeleteMarker = Convert.ToBoolean(httpResponse.Headers[iheaders.DeleteMarkerHeader()]);
            }

            foreach (KeyValuePair<string,string> entry in httpResponse.Headers)
            {
                if (entry.Key != null && entry.Key.StartsWith(iheaders.HeaderMetaPrefix()))
                {
                    response.Metadata.Add(entry.Key.Substring(iheaders.HeaderMetaPrefix().Length), entry.Value);
                }
            }
            if (httpResponse.Headers.ContainsKey(iheaders.RestoreHeader()))
            {
                response.RestoreStatus = new RestoreStatus();
                var restore = httpResponse.Headers[iheaders.RestoreHeader()];
                if (restore.Contains("expiry-date"))
                {
                    var m = Regex.Match(restore, @"ongoing-request=""(?<ongoing>.+)"",\s*expiry-date=""(?<date>.+)""");
                    if (m.Success)
                    {
                        response.RestoreStatus.Restored = !Convert.ToBoolean(m.Groups["ongoing"].Value);
                        response.RestoreStatus.ExpiryDate = CommonUtil.ParseToDateTime(m.Groups["date"].Value, Constants.RFC822DateFormat, Constants.ISO8601DateFormat);
                    }
                }
                else
                {
                    var m = Regex.Match(restore, @"ongoing-request=""(?<ongoing>.+)""");
                    if (m.Success)
                    {
                        response.RestoreStatus.Restored = !Convert.ToBoolean(m.Groups["ongoing"].Value);
                    }
                }
            }

            if (httpResponse.Headers.ContainsKey(iheaders.ExpirationHeader()))
            {
                var expiration = httpResponse.Headers[iheaders.ExpirationHeader()];
                var m = Regex.Match(expiration, @"expiry-date=""(?<date>.+)"",\s*rule-id=""(?<id>.+)""");
                if (m.Success)
                {
                    response.ExpirationDetail            = new ExpirationDetail
                    {
                        RuleId     = m.Groups["id"].Value,
                        ExpiryDate = CommonUtil.ParseToDateTime(m.Groups["date"].Value, Constants.RFC822DateFormat, Constants.ISO8601DateFormat)
                    };
                }
            }

            if (httpResponse.Headers.ContainsKey(iheaders.ObjectTypeHeader()))
            {
                response.Appendable = "Appendable".Equals(httpResponse.Headers[iheaders.ObjectTypeHeader()]);
            }

            if (httpResponse.Headers.ContainsKey(iheaders.NextPositionHeader()))
            {
                response.NextPosition = Convert.ToInt64(httpResponse.Headers[iheaders.NextPositionHeader()]);
            }
        }

        public GetObjectMetadataResponse ParseGetObjectMetadataResponse(HttpResponse httpResponse)
        {
            var response = new GetObjectMetadataResponse();
            ParseGetObjectMetadataResponse(httpResponse, response);
            return response;
        }

        public GetObjectResponse ParseGetObjectResponse(HttpResponse httpResponse)
        {
            var response = new GetObjectResponse();
            ParseGetObjectMetadataResponse(httpResponse, response);
            response.OutputStream = httpResponse.Content;
            return response;
        }

        public AppendObjectResponse ParseAppendObjectResponse(HttpResponse httpResponse)
        {
            var response = new AppendObjectResponse();
            if (httpResponse.Headers.ContainsKey(iheaders.StorageClassHeader()))
            {
                response.StorageClass = ParseStorageClass(httpResponse.Headers[iheaders.StorageClassHeader()]);
            }

            if (httpResponse.Headers.ContainsKey(Constants.CommonHeaders.ETag))
            {
                response.ETag = httpResponse.Headers[Constants.CommonHeaders.ETag];
            }

            if (httpResponse.Headers.ContainsKey(iheaders.NextPositionHeader()))
            {
                response.NextPosition = Convert.ToInt64(httpResponse.Headers[iheaders.NextPositionHeader()]);
            }

            response.ObjectUrl = httpResponse.RequestUrl;

            return response;
        }
    }
}
