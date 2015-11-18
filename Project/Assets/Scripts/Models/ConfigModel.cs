
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;

public class ConfigModel
{
    IDictionary<string, Attr> _globals;
    IList<ScriptModel> _scripts;

    public ConfigModel(IDictionary<string, Attr> globals=null, IList<ScriptModel> scripts=null)
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
    }

    public IEnumerable<ScriptModel> Scripts
    {
        get
        {
            return _scripts;
        }
    }

    public void Assign(ConfigModel other)
    {
        _globals = other._globals;
        _scripts = other._scripts;            
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
        return string.Format("[ConfigModel Globals={0} Scripts={1}]",
          _globals.Count, _scripts.Count);
    }
}
