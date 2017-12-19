using System.Collections.Generic;

namespace SocialPoint.AWS
{
    /// <summary>
    /// Class to implement when defining a new AWS authentication method for query REST requests.
    /// </summary>
    public abstract class AWSQueryAuthenticationMethod
    {
        protected string Method { get; private set; }

        protected string Resource { get; private set; }

        protected string AccessKey { get; private set; }

        protected string Date { get; private set; }

        protected string Zone { get; private set; }

        protected string Service { get; private set; }

        protected string Expire { get; private set; }

        protected string Host { get; private set; }

        protected string SecretKey { get; private set; }

        protected KeyValuePair<string, string>[] Params { get; private set; }

        public abstract string Scope
        {
            get;
        }

        public AWSQueryAuthenticationMethod(string method,
                                             string resource,
                                             string accessKey,
                                             string date,
                                             string zone,
                                             string service,
                                             string expire,
                                             string host,
                                             string secretKey,
                                             KeyValuePair<string, string>[] parms)
        {
            Method = method;
            Resource = resource;
            AccessKey = accessKey;
            Date = date;
            Zone = zone;
            Service = service;
            Expire = expire;
            Params = parms;
            Host = host;
            SecretKey = secretKey;
        }

        public abstract string GetSignature();

        public abstract string GetRequestUrl();
    }
}

