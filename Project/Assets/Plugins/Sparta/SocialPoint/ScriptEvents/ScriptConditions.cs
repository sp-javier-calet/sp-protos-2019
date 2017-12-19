using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.ScriptEvents
{
    public static class ScriptConditions
    {
        public static IChildParser<IScriptCondition>[] BasicParsers
        {
            get
            {
                return new IChildParser<IScriptCondition>[] {
                    new FixedConditionParser(),
                    new AllConditionParser(),
                    new NoneConditionParser(),
                    new NameConditionParser(),
                    new ArgumentsConditionParser(),
                    new AndConditionParser(),
                    new OrConditionParser(),
                    new NotConditionParser()
                };
            }
        }

        public static IAttrObjParser<IScriptCondition> BaseParser
        {
            get
            {
                return new FamilyParser<IScriptCondition>(BasicParsers);
            }
        }
    }

    public sealed class FixedCondition : IScriptCondition
    {
        readonly bool _result;

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

    public sealed class FixedConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new FixedCondition(data.AsValue.ToBool());
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "fixed";

        public string Name { get { return ConditionName; } }
    }

    public sealed class AllConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new FixedCondition(true);
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "all";

        public string Name { get { return ConditionName; } }
    }

    public sealed class NoneConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new FixedCondition(false);
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "none";

        public string Name { get { return ConditionName; } }
    }

    public sealed class NameCondition : IScriptCondition
    {
        readonly string _pattern;

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

    public sealed class NameConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new NameCondition(data.ToString());
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "name";

        public string Name { get { return ConditionName; } }
    }

    public sealed class ArgumentsCondition : IScriptCondition
    {
        readonly Attr _arguments;

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

    public sealed class ArgumentsConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new ArgumentsCondition((Attr)data.Clone());
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "args";

        public string Name { get { return ConditionName; } }
    }

    public sealed class AndCondition : IScriptCondition
    {
        readonly List<IScriptCondition> _conditions;

        public AndCondition(IScriptCondition[] conditions) :
            this(new List<IScriptCondition>(conditions))
        {
        }

        public AndCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }

        public bool Matches(string name, Attr arguments)
        {
            for(int i = 0, _conditionsCount = _conditions.Count; i < _conditionsCount; i++)
            {
                var cond = _conditions[i];
                if(!cond.Matches(name, arguments))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("[AndCondition:{0}]", StringUtils.Join(_conditions));
        }
    }

    public sealed class AndConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            var children = new List<IScriptCondition>();
            var itr = data.AsList.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                children.Add(Parent.Parse(elm));
            }
            itr.Dispose();
            return new AndCondition(children);
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "and";

        public string Name { get { return ConditionName; } }
    }

    public sealed class OrCondition : IScriptCondition
    {
        readonly List<IScriptCondition> _conditions;

        public OrCondition(IScriptCondition[] conditions) :
            this(new List<IScriptCondition>(conditions))
        {
        }

        public OrCondition(List<IScriptCondition> conditions)
        {
            _conditions = conditions;
        }

        public bool Matches(string name, Attr arguments)
        {
            for(int i = 0, _conditionsCount = _conditions.Count; i < _conditionsCount; i++)
            {
                var cond = _conditions[i];
                if(cond.Matches(name, arguments))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("[OrCondition:{0}]", StringUtils.Join(_conditions));
        }
    }

    public sealed class OrConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            var children = new List<IScriptCondition>();
            var itr = data.AsList.GetEnumerator();
            while(itr.MoveNext())
            {
                var elm = itr.Current;
                children.Add(Parent.Parse(elm));
            }
            itr.Dispose();
            return new OrCondition(children);
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "or";

        public string Name { get { return ConditionName; } }
    }

    public sealed class NotCondition : IScriptCondition
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

    public sealed class NotConditionParser : IChildParser<IScriptCondition>
    {
        public IScriptCondition Parse(Attr data)
        {
            return new NotCondition(Parent.Parse(data));
        }

        public FamilyParser<IScriptCondition> Parent { set; private get; }

        const string ConditionName = "not";

        public string Name { get { return ConditionName; } }
    }
}
