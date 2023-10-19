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
using System.IO;
using System.Collections.Generic;
using OBS;
using OBS.Model;

namespace ObsDemo
{
    /// <summary>
    /// This sample demonstrates how to do object-related operations
    /// (such as create/delete/get/copy object, do object ACL) 
    /// on OBS using the OBS SDK for .NET
    /// </summary>
    class ObjectOperationsSample
    {

        private static string endpoint = "https://yyour-endpoint";
        private static string AK = "*** Provide your Access Key ***";
        private static string SK = "*** Provide your Secret Key ***";

        private static ObsClient client;
        private static ObsConfig config;

        private static string bucketName = "my-obs-bucket-demo";
        private static string objectName = "my-obs-object-key-demo";
        private static string versionId = "versionid";
        private static string uploadId;
        private static string etag;
        private static string destobjectName = "destobject";
        private static string filePath = "localfile";
        private static long partSize = 5 * 1024 * 1024;

        [Test]
        public static void Test()
        {

            client = new ObsClient(AK, SK, endpoint);

            //put object
            PutObject();

            //get object
            GetObject();

            //copy object
            CopyObject();

            //get object metadata
            GetObjectMetadata();

            //set object acl
            SetObjectACL();

            //get obeject acl
            GetObjectACL();

            //append object
            AppendObject();

            //restore object
            RestoreObject();

            //initiate multipart upload
            InitiateMultipartUpload();

            //abort multipart upload
            //AbortMultipartUpload();

            //upload part
            UploadPart();

            //copy part
            //CopyPart();

            //list parts
            ListParts();

            //complete multipart upload
            CompleteMultipartUpload();

            //delete object
            DeleteObject();

            //delete objects
            DeleteObjects();

            Console.WriteLine("object opration end...\n");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }


        #region PutObject
        static void PutObject()
        {
            try
            {
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    FilePath = filePath
                };
                var response = client.PutObject(request);

                Console.WriteLine("PutObject response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when put object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetObject
        static void GetObject()
        {
            try
            {
                var request = new GetObjectRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                };

                using var response = client.GetObject(request);
                //save the object to file with specified path
                var dest = "savepath";
                if (!File.Exists(dest))
                {
                    response.WriteResponseStreamToFile(dest);
                }

                Console.WriteLine("Get object response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode{0}, when get object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region CopyObject
        private static void CopyObject()
        {
            try
            {
                var request = new CopyObjectRequest()
                {
                    SourceBucketName = bucketName,
                    SourceObjectKey = objectName,
                    SourceVersionId = versionId,
                    ObjectKey = destobjectName,
                };
                var response = client.CopyObject(request);
                Console.WriteLine("CopyObject response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when copy object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteObject
        static void DeleteObject()
        {
            try
            {
                var request = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    VersionId = versionId,
                };
                var response = client.DeleteObject(request);

                Console.WriteLine("Deleted object response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region DeleteObjects
        private static void DeleteObjects()
        {
            try
            {
                var request = new DeleteObjectsRequest
                {
                    BucketName = bucketName,
                    Quiet      = true
                };
                request.AddKey(objectName);
                request.AddKey(destobjectName);

                var response = client.DeleteObjects(request);

                Console.WriteLine("Delete objects response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when delete objects.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetObjectMetadata
        private static void GetObjectMetadata()
        {
            try
            {
                var request = new GetObjectMetadataRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName
                };
                var response = client.GetObjectMetadata(request);

                Console.WriteLine("Get object metadata response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get object metadata.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region SetObjectACL
        static void SetObjectACL()
        {
            try
            {
                var request = new SetObjectAclRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    CannedAcl = CannedAclEnum.PublicRead
                };

                var response = client.SetObjectAcl(request);

                Console.WriteLine("Set object acl response: {0}.", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when set object acl.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region GetObjectACL
        static void GetObjectACL()
        {
            try
            {
                var request = new GetObjectAclRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName
                };
                var response = client.GetObjectAcl(request);

                Console.WriteLine("Get object acl response: {0}.", response.StatusCode);

                foreach (var grant in response.AccessControlList.Grants)
                {
                    var grantee = (CanonicalGrantee)grant.Grantee;
                    Console.WriteLine("Grantee canonical user id: {0}", grantee.Id);
                    Console.WriteLine("Grantee canonical user display name: {0}", grantee.DisplayName);
                    Console.WriteLine("Grant permission: {0}", grant.Permission);
                }
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get object acl.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region AppendObject
        static void AppendObject()
        {
            try
            {
                var request = new AppendObjectRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    FilePath = filePath,
                    Position = 10
                };
                var response = client.AppendObject(request);

                Console.WriteLine("Append object response: {0}", response.StatusCode);
                Console.WriteLine("ETag: {0}", response.ETag);
                Console.WriteLine("NextPosition: {0}", response.NextPosition.ToString());
                Console.WriteLine("Object StorageClass: {0}", response.StorageClass.ToString());
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when append object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region RestoreObject
        private static void RestoreObject()
        {
            try
            {
                var request = new RestoreObjectRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    Days = 5,
                    Tier = RestoreTierEnum.Expedited,
                    VersionId = versionId
                };
                var response = client.RestoreObject(request);

                Console.WriteLine("Get restore object response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when get restore object.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region InitiateMultipartUpload
        static void InitiateMultipartUpload()
        {
            try
            {
                var request = new InitiateMultipartUploadRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                };
                var response = client.InitiateMultipartUpload(request);

                Console.WriteLine("Initiate multipart upload response: {0}", response.StatusCode);
                Console.WriteLine("upload id: {0}", response.UploadId);
                uploadId = response.UploadId;
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when initiate multipart upload.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region AbortMultipartUpload
        static void AbortMultipartUpload()
        {
            try
            {
                var request = new AbortMultipartUploadRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    UploadId = uploadId
                };
                var response = client.AbortMultipartUpload(request);

                Console.WriteLine("Abort multipart upload response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when abort multipart upload.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region CompleteMultipartUpload
        static void CompleteMultipartUpload()
        {
            try
            {
                List<PartETag> partEtags = new List<PartETag>();
                var            partEtag1 = new PartETag
                {
                    PartNumber = 1,
                    ETag       = etag
                };
                partEtags.Add(partEtag1);

                var request = new CompleteMultipartUploadRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    UploadId = uploadId,
                    PartETags = partEtags
                };
                var response = client.CompleteMultipartUpload(request);

                Console.WriteLine("Complete multipart upload response: {0}", response.StatusCode);
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when complete multipart upload.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region UploadPart
        static void UploadPart()
        {
            try
            {
                var request = new UploadPartRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    FilePath = filePath,
                    PartNumber = 1,
                    PartSize = partSize,
                    UploadId = uploadId,
                    Offset = 100,
                };
                var response = client.UploadPart(request);

                Console.WriteLine("UploadPart response: {0}", response.StatusCode);
                Console.WriteLine("ETag: {0}", response.ETag);
                etag = response.ETag;
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when upload part.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region CopyPart
        static void CopyPart()
        {
            try
            {
                var range = new ByteRange(10, 20);

                var request = new CopyPartRequest()
                {
                    SourceBucketName = bucketName,
                    SourceObjectKey = objectName,
                    SourceVersionId = versionId,
                    ObjectKey = destobjectName,
                    PartNumber = 1,
                    UploadId = uploadId,
                    ByteRange = range
                };
                var response = client.CopyPart(request);

                Console.WriteLine("Copy part response: {0}", response.StatusCode);
                Console.WriteLine("ETag: {0}", response.ETag);
                etag = response.ETag;
            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when copy part.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion

        #region ListParts
        static void ListParts()
        {
            try
            {
                var request = new ListPartsRequest()
                {
                    BucketName = bucketName,
                    ObjectKey = objectName,
                    UploadId = uploadId,
                    MaxParts = 10,
                    PartNumberMarker = 1,
                };
                var response = client.ListParts(request);

                Console.WriteLine("List parts response: {0}", response.StatusCode);
                Console.WriteLine("Lis parts count: " + response.Parts.Count);

                foreach(PartETag part in response.Parts)
                {
                    Console.WriteLine("PartNumber: {0}", part.PartNumber);
                    Console.WriteLine("ETag: {0}", part.ETag);
                }

            }
            catch (ObsException ex)
            {
                Console.WriteLine("Exception errorcode: {0}, when list parts.", ex.ErrorCode);
                Console.WriteLine("Exception errormessage: {0}", ex.ErrorMessage);
            }
        }
        #endregion


    }
}