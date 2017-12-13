

using System;

namespace SocialPoint.AWS.S3
{
    public class S3ListObject
    {
        public string Key { get; private set; }
        public DateTime LastModified { get; private set; }
        public string eTag { get; private set; }
        public int size { get; private set; }
        public string storageClass { get; private set; }
        public bool IsFolder
        {
            get
            {
                return System.IO.Path.GetExtension(Key) == string.Empty;

            }
            private set { }
        }

        public S3ListObject(string key, string dateString, string eTag, string size, string storageClass)
        {
            Key = key;
            LastModified = DateTime.Parse(dateString);
            this.eTag = eTag;
            this.size = int.Parse(size);
            this.storageClass = storageClass;
        }
    }
}

