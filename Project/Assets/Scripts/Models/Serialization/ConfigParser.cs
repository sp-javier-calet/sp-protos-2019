using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;

public class ConfigParser : IParser<ConfigModel>
{
    const string AttrKeyGame = "game";
    const string AttrKeyResources = "resources";
    const string AttrKeyStore = "store";
    const string AttrKeyGlobals = "globals";
    const string AttrKeyScripts = "scripts";
    const string AttrKeyGlobalKey = "key";
    const string AttrKeyGlobalValue = "value";

    ConfigModel _config;
    IParser<ScriptModel> _scriptParser;
    IParser<StoreModel> _storeParser;

    public ConfigParser(ConfigModel config, IParser<StoreModel> storeParser, IParser<ScriptModel> scriptParser)
    {
        _config = config;
        _scriptParser = scriptParser;
        _storeParser = storeParser;
    }

    public ConfigModel Parse(Attr data)
    {
        var globals = ParseGlobals(data);
        var scripts = ParseScripts(data);

        var resourceTypes = new ResourceTypesParser().Parse(data.AsDic[AttrKeyResources]);
        _storeParser.Parse(data.AsDic[AttrKeyStore]);

        return _config.Init(globals, scripts, resourceTypes);
    }

    Dictionary<string, Attr> ParseGlobals(Attr data)
    {
        var globalsDict = data.AsDic[AttrKeyGlobals];
        var globals = new Dictionary<string, Attr>();

        if(globalsDict.AttrType == AttrType.DICTIONARY)
        {
            foreach(var gAttr in globalsDict.AsDic)
            {
                globals[gAttr.Key] = (Attr)gAttr.Value.Clone();
            }
        }
        else if(globalsDict.AttrType == AttrType.LIST)
        {
            foreach(var gAttr in globalsDict.AsList)
            {
                var gAttrdic = gAttr.AsDic;
                var key = gAttrdic[AttrKeyGlobalKey].AsValue.ToString();
                if(!string.IsNullOrEmpty(key))
                {
                    globals[key] = (Attr)gAttrdic[AttrKeyGlobalValue].Clone();
                }
            }
        }

        return globals;
    }

    List<ScriptModel> ParseScripts(Attr data)
    {
        var scripts = new List<ScriptModel>();

        if(_scriptParser != null)
        {
            foreach(var script in data.AsDic[AttrKeyScripts].AsList)
            {
                scripts.Add(_scriptParser.Parse(script));
            }
        }

        return scripts;
    }
}
