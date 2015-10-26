
using System.Collections.Generic;
using SocialPoint.Attributes;

public class ConfigModel
{
    IDictionary<string, Attr> _globals;

    public ConfigModel(IDictionary<string, Attr> globals = null)
    {
        _globals = globals;
    }

    public Attr GetGlobal(string name)
    {
        Attr value;
        if(_globals == null || !_globals.TryGetValue(name, out value))
        {
            return null;
        }
        return value;
    }

    public override string ToString()
    {
        return string.Format("[ConfigModel]");
    }
}
