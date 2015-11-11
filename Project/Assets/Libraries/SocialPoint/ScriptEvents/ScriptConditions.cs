using SocialPoint.Attributes;
using SocialPoint.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SocialPoint.ScriptEvents
{
    public class NameCondition : IScriptCondition
    {
        string _evNamePattern;

        public NameCondition(string evNamePattern)
        {
            _evNamePattern = evNamePattern;
        }

        public bool Matches(string evName, Attr evArguments)
        {
            return StringUtils.GlobMatch(_evNamePattern, evName);
        }
    }

    public class EventNameConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new NameCondition(data.ToString());
        }

        public void Load(FamilyParser<IScriptCondition> parent)
        {
        }

        const string ConditionName = "name";      
        public string Name { get{ return ConditionName; } }
    }

    public class ArgumentsCondition : IScriptCondition
    {
        Attr _evArguments;

        public ArgumentsCondition(Attr evArguments)
        {
            _evArguments = evArguments;
        }

        public bool Matches(string evName, Attr evArguments)
        {
            return evArguments == _evArguments;
        }
    }

    public class EventArgumentsConditionParser : IParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new ArgumentsCondition((Attr)data.Clone());
        }
                
        public void Load(FamilyParser<IScriptCondition> parent)
        {
        }
        
        const string ConditionName = "args";      
        public string Name { get{ return ConditionName; } }
    }

    public class AndCondition : IScriptCondition
    {
        List<IScriptCondition> _conditions;

        public AndCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }
        
        public bool Matches(string evName, Attr evArguments)
        {
            foreach(var cond in _conditions)
            {
                if(!cond.Matches(evName, evArguments))
                {
                    return false;
                }
            }
            return true;
        }
    }
        
    public class AndConditionParser : IParser<IScriptCondition>
    {
        FamilyParser<IScriptCondition> _parent;

        public IScriptCondition Parse(Attr data)
        {
            var children = new List<IScriptCondition>();
            foreach(var elm in data.AsList)
            {
                children.Add(_parent.Parse(elm));
            }
            return new AndCondition(children);
        }
        
        public void Load(FamilyParser<IScriptCondition> parent)
        {
            _parent = parent;
        }
        
        const string ConditionName = "and";      
        public string Name { get{ return ConditionName; } }
    }

    public class OrCondition : IScriptCondition
    {
        List<IScriptCondition> _conditions;
        
        public OrCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }
        
        public bool Matches(string evName, Attr evArguments)
        {
            foreach(var cond in _conditions)
            {
                if(cond.Matches(evName, evArguments))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class OrConditionParser : IParser<IScriptCondition>
    {
        FamilyParser<IScriptCondition> _parent;
        
        public IScriptCondition Parse(Attr data)
        {
            var children = new List<IScriptCondition>();
            foreach(var elm in data.AsList)
            {
                children.Add(_parent.Parse(elm));
            }
            return new OrCondition(children);
        }
        
        void Load(FamilyParser<IScriptCondition> parent)
        {
            _parent = parent;
        }
        
        const string ConditionName = "or";      
        public string Name { get{ return ConditionName; } }
    }

    public class NotCondition : IScriptCondition
    {
        IScriptCondition _condition;
        
        public NotCondition(IScriptCondition condition)
        {
            _condition = condition;
        }
        
        public bool Matches(string evName, Attr evArguments)
        {
            return !_condition.Matches(evName, evArguments);
        }
    }
        
    public class NotConditionParser : IParser<IScriptCondition>
    {
        FamilyParser<IScriptCondition> _parent;
        
        public IScriptCondition Parse(Attr data)
        {
            return new NotCondition(_parent.Parse(data));
        }
        
        public void Load(FamilyParser<IScriptCondition> parent)
        {
            _parent = parent;
        }
        
        const string ConditionName = "not";      
        public string Name { get{ return ConditionName; } }
    }
}