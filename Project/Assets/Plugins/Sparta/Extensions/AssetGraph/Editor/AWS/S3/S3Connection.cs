using UnityEngine;


namespace SocialPoint.AWS.S3
{
    public class S3Connection
    {
        public const string EXPIRE = "86400";
        public const string HOST = "s3.amazonaws.com";
        public const string SERVICE = "s3";
        public const string ZONE = "us-east-1";

        public string AccessKey { get; private set; }

        public string SecretKey { get; private set; }

        public S3Connection(string accessKey, string secretKey)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
        }

        public BucketS3 GetBucket(string bucketName)
        {
            BucketS3 bucket = new BucketS3(AccessKey, SecretKey, bucketName);
            return bucket;
        }
    }
}

