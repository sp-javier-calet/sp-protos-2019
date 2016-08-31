using System;
using LitJson;
using SocialPoint.Attributes;

namespace SocialPoint.Tool.Shared
{
    public sealed class SessionResponse
    {
        public string       success;
        public bool         Success { get { return Boolean.Parse(success); } }
        public Response     response;

        public static SessionResponse FromAttr(Attr response)
        {
            var serializer = new LitJsonAttrSerializer();
            var writer = new JsonWriter();
            serializer.Serialize(response, writer);
            return FromJson(writer.ToString());
        }
        
        public static SessionResponse FromJson(string response)
        {
            var sessionResponse = JsonMapper.ToObject<SessionResponse>(response);
            return sessionResponse;
        }

        public sealed class Response
        {
            public string   session_id;
            public string   email;
        }
    }
}

