using SocialPoint.Attributes;

public class ConfigPatchParser : IAttrObjParser<ConfigPatch>
{
    #region IParser implementation

    public ConfigPatch Parse(Attr data)
    {
        return new ConfigPatch(data.AsList);
    }

    #endregion
}


