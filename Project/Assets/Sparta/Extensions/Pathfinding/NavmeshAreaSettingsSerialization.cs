using System.Collections;
using SocialPoint.IO;

namespace SocialPoint.Pathfinding
{
    public class NavmeshAreaSettingsSerializer : IWriteSerializer<NavmeshAreaSettings>
    {
        public static readonly NavmeshAreaSettingsSerializer Instance = new NavmeshAreaSettingsSerializer();

        public void Serialize(NavmeshAreaSettings value, IWriter writer)
        {
            value.Serialize(writer);
        }
    }

    public class NavmeshAreaSettingsParser : IReadParser<NavmeshAreaSettings>
    {
        public static readonly NavmeshAreaSettingsParser Instance = new NavmeshAreaSettingsParser();

        public NavmeshAreaSettings Parse(IReader reader)
        {
            var areaSettings = new NavmeshAreaSettings();
            areaSettings.Deserialize(reader);
            return areaSettings;
        }
    }
}