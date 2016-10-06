using System;
using SharpNav.Pathfinding;

/// <summary>
/// Class based in the original file from SharpNav/IO/Json.
/// </summary>
namespace SocialPoint.Pathfinding
{
    class PolyIdConverter
    {
        /*public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NavPolyId);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.Null)
                return null;

            return new NavPolyId(serializer.Deserialize<int>(reader));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var polyId = (value as NavPolyId?).Value;
            serializer.Serialize(writer, (int)polyId.Id);
        }*/
    }
}
