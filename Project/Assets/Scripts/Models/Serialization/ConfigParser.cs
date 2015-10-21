using SocialPoint.Attributes;

public class ConfigParser : IParser<ConfigModel>
{
    public ConfigModel Parse(Attr data)
    {
        var model = new ConfigModel();
        return model;
    }
}
