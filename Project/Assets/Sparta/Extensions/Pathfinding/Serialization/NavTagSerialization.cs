using System.Collections;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Pathfinding
{
    public class NavTagSerializer : IWriteSerializer<object>
    {
        public static readonly NavTagSerializer Instance = new NavTagSerializer();

        public void Serialize(object value, IWriter writer)
        {
            var tagSet = value as TagSet;
            if(tagSet != null)
            {
                writer.WriteStringArray(tagSet.ToArray());
            }
            else
            {
                writer.WriteStringArray(new string[] { });
            }
        }
    }

    public class NavTagParser : IReadParser<object>
    {
        public static readonly NavTagParser Instance = new NavTagParser();

        public object Parse(IReader reader)
        {
            var tagSet = new TagSet();
            var tags = reader.ReadStringArray();
            for(int i = 0; i < tags.Length; i++)
            {
                tagSet.Add(tags[i]);
            }
            return tagSet;
        }
    }
}
