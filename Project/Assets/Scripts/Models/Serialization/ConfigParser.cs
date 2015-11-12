using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;

public class ConfigParser : IParser<ConfigModel>
{
    const string AttrKeyGlobals = "globals";
    const string AttrKeyScripts = "scripts";
    const string AttrKeyGlobalKey = "key";
    const string AttrKeyGlobalValue = "value";

    IParser<ScriptModel> _scriptParser;

    public ConfigParser(IParser<ScriptModel> scriptParser)
    {
        _scriptParser = scriptParser;
    }

    public ConfigModel Parse(Attr data)
    {
        var globals = new Dictionary<string, Attr>();
        var scripts = new List<ScriptModel>();
        var gsAttr = data.AsDic[AttrKeyGlobals];

        if(gsAttr.AttrType == AttrType.DICTIONARY)
        {
            foreach(var gAttr in gsAttr.AsDic)
            {
                globals[gAttr.Key] = (Attr) gAttr.Value.Clone();
            }
        }
        else if(gsAttr.AttrType == AttrType.LIST)
        {
            foreach(var gAttr in gsAttr.AsList)
            {
                var gAttrdic = gAttr.AsDic;
                var key = gAttrdic[AttrKeyGlobalKey].AsValue.ToString();
                if(!string.IsNullOrEmpty(key))
                {
                    globals[key] = (Attr)gAttrdic[AttrKeyGlobalValue].Clone();
                }
            }
        }
        foreach(var script in data.AsDic[AttrKeyScripts].AsList)
        {
            scripts.Add(_scriptParser.Parse(script));
        }
        return new ConfigModel(globals, scripts);
    }
}
