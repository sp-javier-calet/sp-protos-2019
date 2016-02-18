
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;

public class ConfigModel
{
    IDictionary<string, Attr> _globals;
    IList<ScriptModel> _scripts;
    IDictionary<string, ResourceType> _resourceTypes;
    StoreModel _store;

    public event Action<ConfigModel> Assigned;

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

    public ConfigModel(IDictionary<string, Attr> globals = null, 
                       IList<ScriptModel> scripts = null,
                       IDictionary<string, ResourceType> resourceTypes = null,
                       StoreModel store = null)
    {
        if(globals == null)
        {
            globals = new Dictionary<string, Attr>();
        }
        _globals = globals;

        if(scripts == null)
        {
            scripts = new List<ScriptModel>();
        }
        _scripts = scripts;

        if(resourceTypes == null)
        {
            resourceTypes = new Dictionary<string, ResourceType>();
        }
        _resourceTypes = resourceTypes;

        if(store == null)
        {
            store = new StoreModel();
        }
        _store = store;
    }

    public void Assign(ConfigModel other)
    {
        _globals = other._globals;
        _scripts = other._scripts;
        _resourceTypes = other._resourceTypes;
        _store.Assign(other._store);
        if(Assigned != null)
        {
            Assigned(this);
        }
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
}
