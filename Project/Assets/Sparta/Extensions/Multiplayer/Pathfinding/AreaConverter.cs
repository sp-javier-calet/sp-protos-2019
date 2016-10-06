using System;
using SharpNav;

/// <summary>
/// Class based in the original file from SharpNav/IO/Json.
/// </summary>
namespace SocialPoint.Pathfinding
{
    public class AreaConverter
    {
        /*public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Area);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.Null)
                return null;

            return new Area(serializer.Deserialize<byte>(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var area = (value as Area?).Value;
            serializer.Serialize(writer, (int)area.Id);
        }*/
    }
}
