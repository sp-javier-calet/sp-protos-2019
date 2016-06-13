
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;

public class ConfigModel : IDisposable
{
    IDictionary<string, Attr> _globals;
    IList<ScriptModel> _scripts;
    IDictionary<string, ResourceType> _resourceTypes;
    StoreModel _store;

    public IEnumerable<ScriptModel> Scripts
    {
        get
        {
            return _scripts;
        }
    }

    public IEnumerable<KeyValuePair<string, ResourceType>> ResourceTypes
    {
        get
        {
            return _resourceTypes;
        }
    }

    public StoreModel Store
    {
        get
        {
            return _store;
        }
    }

    public ConfigModel()
    {
        _store = new StoreModel();
    }

    public ConfigModel Init(IDictionary<string, Attr> globals, 
                            IList<ScriptModel> scripts,
                            IDictionary<string, ResourceType> resourceTypes)
    {
        _globals = globals;
        _scripts = scripts;
        _resourceTypes = resourceTypes;

        return this;
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
        return string.Format("[ConfigModel: Globals={0}, Scripts={1}, Resources={2}, Store={3}]",
            _globals.Count, _scripts.Count, _resourceTypes.Count, _store.ToString());
    }

    public void Dispose()
    {
        
    }
}
