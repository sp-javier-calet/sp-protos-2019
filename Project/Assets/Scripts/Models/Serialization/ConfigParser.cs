using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;

public class ConfigParser : IAttrObjParser<ConfigModel>
{
    const string AttrKeyGame = "game";
    const string AttrKeyResources = "resources";
    const string AttrKeyStore = "store";
    const string AttrKeyGlobals = "globals";
    const string AttrKeyScripts = "scripts";
    const string AttrKeyGoals = "goals";
    const string AttrKeyGlobalKey = "key";
    const string AttrKeyGlobalValue = "value";

    ConfigModel _config;
    IAttrObjParser<ScriptModel> _scriptParser;
    IAttrObjParser<StoreModel> _storeParser;
    IAttrObjParser<GoalsTypeModel> _goalsParser;

    public ConfigParser(ConfigModel config, IAttrObjParser<StoreModel> storeParser, IAttrObjParser<GoalsTypeModel> goalsParser, IAttrObjParser<ScriptModel> scriptParser)
    {
        _config = config;
        _scriptParser = scriptParser;
        _storeParser = storeParser;
        _goalsParser = goalsParser;
    }

    public ConfigModel Parse(Attr data)
    {
        var globals = ParseGlobals(data);
        var scripts = ParseScripts(data);

        var resourceTypes = new ResourceTypesParser().Parse(data.AsDic[AttrKeyResources]);
        _storeParser.Parse(data.AsDic[AttrKeyStore]);

        var goals = _goalsParser.Parse(data.AsDic[AttrKeyGoals]);

        return _config.Init(globals, scripts, resourceTypes, goals);
    }

    Dictionary<string, Attr> ParseGlobals(Attr data)
    {
        //In Config Manager the first "globals" is the tab and the second "globals" is the Page
        //Ex: -> {"globals": {"globals": [{"key":"cube_speed","value":2}]}}
        var globalsDict = data.AsDic[AttrKeyGlobals].AsDic[AttrKeyGlobals];
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
