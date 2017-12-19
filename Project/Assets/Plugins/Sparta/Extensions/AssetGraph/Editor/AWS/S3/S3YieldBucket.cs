using System;
using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Net;
using System.Net.Security;

namespace SocialPoint.AWS.S3
{
    public class BucketS3
    {
        public const long LISTKEYS_TIMEOUT = 10000;

        string accessKey;
        string secretKey;
        string bucketName;

        public BucketS3(string _accessKey, string _secretKey, string _bucketName)
        {
            accessKey = _accessKey;
            secretKey = _secretKey;
            bucketName = _bucketName;
        }

        public List<S3ListObject> ListFiles(string prefix = "")
        {
            List<S3ListObject> keysResult = new List<S3ListObject>();

            string resource = "/";

            KeyValuePair<string, string>[] parms = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("list-type", "2"), new KeyValuePair<string, string>("prefix", prefix) };

            var text = ExecuteAction(resource, parms);

            using(XmlReader reader = XmlReader.Create(new StringReader(text)))
            {
                reader.ReadToFollowing("ListBucketResult");
                reader.ReadToDescendant("MaxKeys");
                int maxKeys = reader.ReadElementContentAsInt();
                if(reader.Name != "IsTruncated")
                {
                    reader.ReadToNextSibling("IsTruncated");
                }
                bool isTruncated = reader.ReadElementContentAsBoolean();
                if(isTruncated)
                {
                    Debug.LogWarning(String.Format("S3Bucket '{0}' ListKeys: Truncated results, listing only {1} keys from total", bucketName, maxKeys));
                }

                XmlReaderSettings xst = new XmlReaderSettings();
                xst.ConformanceLevel = ConformanceLevel.Auto;

                if(reader.Name != "Contents")
                {
                    reader.ReadToNextSibling("Contents");
                }

                do
                {
                    if(reader.ReadToFollowing("Key"))
                    {
                        var key = reader.ReadElementString();
                        var date = reader.ReadElementString();
                        var tag = reader.ReadElementString();
                        var size = reader.ReadElementString();
                        var storageClass = reader.ReadElementString();

                        keysResult.Add(new S3ListObject(key, date, tag, size, storageClass));
                    }
                } while(reader.Name == "Contents");
            }


            return keysResult;
        }

        public string DownloadFile(string path)
        {
            string resource = "/" + path;

            KeyValuePair<string, string>[] parms = new KeyValuePair<string, string>[0];

            var text = ExecuteAction(resource, parms);

            return text;
        }

        string GetUrl(string resource, KeyValuePair<string, string>[] parms)

        {
            AWSQueryAuthenticationMethod auth = new AWSQuerySignatureVersion4(
                "GET",
                resource,
                accessKey,
                DateTime.UtcNow.ToString("yyyyMMdd"),
                S3Connection.ZONE,
                S3Connection.SERVICE,
                S3Connection.EXPIRE,
                bucketName + "." + S3Connection.HOST,
                secretKey,
                parms
                );

            return auth.GetRequestUrl();
        }

        string ExecuteAction(string resource, KeyValuePair<string, string>[] queryParams)
        {
            string url = GetUrl(resource, queryParams);

            RemoteCertificateValidationCallback SSLDelegate = (x, y, z, w) => true;

            ServicePointManager.ServerCertificateValidationCallback += SSLDelegate;
            WebRequest req = HttpWebRequest.Create(url);

            var response = req.GetResponse();

            string responseBody = string.Empty;
            using(var reader = new StreamReader(response.GetResponseStream()))
            {
                responseBody = reader.ReadToEnd();
            }
            response.Close();

            ServicePointManager.ServerCertificateValidationCallback -= SSLDelegate;

            return responseBody;
        }
    }
}

