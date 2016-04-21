using System;
using LitJson;
using SocialPoint.Attributes;

namespace SocialPoint.Editor.SPAMGui
{
    public class AuthResponse
    {
        public string   success;
        public bool     Success { get { return Boolean.Parse(success); } }
        public string   is_recoverable;
        public bool     IsRecoverable { get { return Boolean.Parse(is_recoverable); } }
        public string   message;

        static AuthResponse()
        {
            JsonMapper.RegisterImporter<String, Boolean>(Boolean.Parse);
        }

        public static AuthResponse FromAttr(Attr response)
        {
            var serializer = new LitJsonAttrSerializer();
            var writer = new JsonWriter();
            serializer.Serialize(response, writer);
            return FromJson(writer.ToString());
        }

        public static AuthResponse FromJson(string response)
        {
            var authResponse = JsonMapper.ToObject<AuthResponse>(response);
            return authResponse;
        }
    }
}
