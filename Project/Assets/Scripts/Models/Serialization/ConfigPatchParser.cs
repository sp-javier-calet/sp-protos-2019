using SocialPoint.Attributes;

public class ConfigPatchParser : IParser<ConfigPatch>
{
    #region IParser implementation

    public ConfigPatch Parse(Attr data)
    {
        return new ConfigPatch(data.AsList);
    }

    #endregion
}


