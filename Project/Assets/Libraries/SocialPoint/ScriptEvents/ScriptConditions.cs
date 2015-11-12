using SocialPoint.Attributes;
using SocialPoint.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SocialPoint.ScriptEvents
{
    public class FixedCondition : IScriptCondition
    {
        bool _result;
        
        public FixedCondition(bool result)
        {
            _result = result;
        }
        
        public bool Matches(string name, Attr arguments)
        {
            return _result;
        }
        
        public override string ToString()
        {
            return string.Format("[FixedCondition:{0}]", _result);
        }
    }
    
    public class FixedConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new FixedCondition(data.AsValue.ToBool());
        }
        
        public void Load(FamilyParser<IScriptCondition> parent)
        {
        }
        
        const string ConditionName = "fixed";      
        public string Name { get{ return ConditionName; } }
    }

    public class NameCondition : IScriptCondition
    {
        string _pattern;

        public NameCondition(string pattern)
        {
            _pattern = pattern;
        }

        public bool Matches(string name, Attr arguments)
        {
            return StringUtils.GlobMatch(_pattern, name);
        }

        public override string ToString()
        {
            return string.Format("[NameCondition:{0}]", _pattern);
        }
    }

    public class NameConditionParser : IChildParser<IScriptCondition>
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
        Attr _arguments;

        public ArgumentsCondition(Attr arguments)
        {
            _arguments = arguments;
        }

        public bool Matches(string name, Attr arguments)
        {
            return arguments == _arguments;
        }

        public override string ToString()
        {
            return string.Format("[ArgumentsCondition:{0}]", _arguments);
        }
    }

    public class ArgumentsConditionParser : IParser<IScriptCondition>
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
        readonly List<IScriptCondition> _conditions;

        public AndCondition(IScriptCondition[] conditions):
            this(new List<IScriptCondition>(conditions))
        {
        }

        public AndCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }
        
        public bool Matches(string name, Attr arguments)
        {
            foreach(var cond in _conditions)
            {
                if(!cond.Matches(name, arguments))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var str = new string[_conditions.Count];
            var i = 0;
            foreach(var cond in _conditions)
            {
                str[i] = cond.ToString();
            }
            return string.Format("[AndCondition:{0}]", string.Join(", ", str));
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
        readonly List<IScriptCondition> _conditions;

        public OrCondition(IScriptCondition[] conditions):
            this(new List<IScriptCondition>(conditions))
        {
        }
        
        public OrCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }
        
        public bool Matches(string name, Attr arguments)
        {
            foreach(var cond in _conditions)
            {
                if(cond.Matches(name, arguments))
                {
                    return true;
                }
            }
            return false;
        }
                
        public override string ToString()
        {
            var str = new string[_conditions.Count];
            var i = 0;
            foreach(var cond in _conditions)
            {
                str[i] = cond.ToString();
            }
            return string.Format("[OrCondition:{0}]", string.Join(", ", str));
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
        readonly IScriptCondition _condition;
        
        public NotCondition(IScriptCondition condition)
        {
            _condition = condition;
        }
        
        public bool Matches(string name, Attr arguments)
        {
            return !_condition.Matches(name, arguments);
        }

        public override string ToString()
        {
            return string.Format("[NotCondition:{0}]", _condition);
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