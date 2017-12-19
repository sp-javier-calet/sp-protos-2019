using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SocialPoint.Utils;

namespace SocialPoint.AWS
{
    public class AWSQuerySignatureVersion4 : AWSQueryAuthenticationMethod
    {
        public override string Scope
        {
            get
            {
                return Date + "/" + Zone + "/" + Service + "/" + "aws4_request";
            }
        }

        public AWSQuerySignatureVersion4(string method,
                                          string resource,
                                          string accessKey,
                                          string date,
                                          string zone,
                                          string service,
                                          string expire,
                                          string host,
                                          string secretKey,
                                          KeyValuePair<string, string>[] parms) : base(method, resource, accessKey, date, zone, service, expire, host, secretKey, parms)
        {
        }

        public string GetCanonicalRequest()
        {
            string cr = Method + "\n"
                + Resource + "\n"
                    + "X-Amz-Algorithm=AWS4-HMAC-SHA256" + "&"
                    + "X-Amz-Credential=" + Uri.EscapeDataString(AccessKey + "/" + Scope) + "&"
                    + "X-Amz-Date=" + StringUtils.GetIsoTimeStr(Date) + "&"
                    + "X-Amz-Expires=" + Expire + "&"
                    + "X-Amz-SignedHeaders=host"
                    + StringUtils.GetJoinedUrlParams(Params) + "\n"
                    + "host:" + Host + "\n"
                    + "\n"
                    + "host" + "\n"
                    + "UNSIGNED-PAYLOAD";

            return cr;
        }

        public string GetStringToSign()
        {
            string cr = GetCanonicalRequest();
            SHA256 m = SHA256.Create();
            string crh = BitConverter.ToString(m.ComputeHash(Encoding.UTF8.GetBytes(cr))).Replace("-", "").ToLower();

            string s2s = "AWS4-HMAC-SHA256" + "\n"
                + StringUtils.GetIsoTimeStr(Date) + "\n"
                    + Scope + "\n"
                    + crh;

            return s2s;
        }

        public override string GetSignature()
        {
            HMACSHA256 m = new HMACSHA256(Encoding.UTF8.GetBytes("AWS4" + SecretKey));
            byte[] b = m.ComputeHash(Encoding.UTF8.GetBytes(Date));

            m = new HMACSHA256(b);
            byte[] b2 = m.ComputeHash(Encoding.UTF8.GetBytes(Zone));

            m = new HMACSHA256(b2);
            byte[] b3 = m.ComputeHash(Encoding.UTF8.GetBytes(Service));

            m = new HMACSHA256(b3);
            byte[] b4 = m.ComputeHash(Encoding.UTF8.GetBytes("aws4_request"));

            m = new HMACSHA256(b4);
            string signature = BitConverter.ToString(m.ComputeHash(Encoding.UTF8.GetBytes(GetStringToSign()))).Replace("-", "").ToLower();

            return signature;
        }

        public override string GetRequestUrl()
        {
            string url = "http://" + Host + Resource
                + "?X-Amz-Algorithm=AWS4-HMAC-SHA256"
                    + "&X-Amz-Credential=" + Uri.EscapeDataString(AccessKey + "/" + Scope)
                    + "&X-Amz-Date=" + StringUtils.GetIsoTimeStr(Date)
                    + "&X-Amz-Expires=" + Expire
                    + "&X-Amz-SignedHeaders=host"
                    + "&X-Amz-Signature=" + GetSignature()
                    + StringUtils.GetJoinedUrlParams(Params);

            return url;
        }
    }
}

