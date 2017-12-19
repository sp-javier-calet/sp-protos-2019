using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public delegate void ConsoleCommandDelegate(ConsoleCommand cmd);

    public class ConsoleCommand : IEnumerable<ConsoleCommandOption>
    {
        public ConsoleCommandDelegate Delegate;
        public string Description;
        public IEnumerable<string> Arguments;
        IList<ConsoleCommandOption> _options = new List<ConsoleCommandOption>();

        public IEnumerator<ConsoleCommandOption> GetEnumerator()
        {
            return _options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ConsoleCommandOption this[string value]
        {
            get
            {
                for(int i = 0, _optionsCount = _options.Count; i < _optionsCount; i++)
                {
                    var opt = _options[i];
                    for(int j = 0, optNamesLength = opt.Names.Length; j < optNamesLength; j++)
                    {
                        var name = opt.Names[j];
                        if(name == value)
                        {
                            return opt;
                        }
                    }
                }
                return null;
            }
        }

        public ConsoleCommand WithOptions(IList<ConsoleCommandOption> opts)
        {
            for(int i = 0, optsCount = opts.Count; i < optsCount; i++)
            {
                var opt = opts[i];
                WithOption(opt);
            }
            return this;
        }

        public ConsoleCommand WithOption(ConsoleCommandOption opt)
        {
            _options.Add(opt);
            return this;
        }

        public ConsoleCommand WithDescription(string desc)
        {
            Description = desc;
            return this;
        }

        public ConsoleCommand WithDelegate(ConsoleCommandDelegate dlg)
        {
            Delegate = dlg;
            return this;
        }

        virtual public void Define()
        {
        }

        static int MatchRepeat(string all, string part)
        {
            int f = all.Length / part.Length;
            var parts = string.Empty;
            for(int i = 0; i < f; i++)
            {
                parts += part;
            }
            return all == parts ? f : 0;
        }

        bool SetOptionValue(string name, string value)
        {
            var option = this[name];
            if(option != null)
            {
                option.Value = value;
            }
            if(option == null && string.IsNullOrEmpty(value))
            {
                for(int i = 0, _optionsCount = _options.Count; i < _optionsCount; i++)
                {
                    var opt = _options[i];
                    for(int j = 0, optNamesLength = opt.Names.Length; j < optNamesLength; j++)
                    {
                        var optName = opt.Names[j];
                        int id = MatchRepeat(name, optName);
                        if(id > 0)
                        {
                            opt.Value = id.ToString();
                        }
                    }
                }
            }
            return option != null;
        }

        const string WildcardString = "*";
        const char OptionStartChar = '-';
        const string OptionValueOperator = "=";
        const string ArgSeparator = " ";

        public void SetOptionValues(IEnumerable<string> args)
        {
            Arguments = args;
            string lastOpt = null;
            int i = 0;

            string defVal = null;
            for(int j = 0, _optionsCount = _options.Count; j < _optionsCount; j++)
            {
                var opt = _options[j];
                var parts = new List<string>(opt.Names);
                if(parts.Contains(WildcardString))
                {
                    defVal = string.Empty;
                    break;
                }
            }

            var itr = args.GetEnumerator();
            while(itr.MoveNext())
            {
                var arg = itr.Current;
                if(arg.Length > 0 && arg[0] == OptionStartChar)
                {
                    lastOpt = null;
                    var opt = arg.Trim(new []{ OptionStartChar });
                    var p = opt.IndexOf(OptionValueOperator);
                    if(p == -1)
                    {
                        if(!SetOptionValue(opt, string.Empty))
                        {
                            lastOpt = opt;
                        }
                    }
                    else
                    {
                        SetOptionValue(opt.Substring(0, p), opt.Substring(p + 1));
                    }
                }
                else if(lastOpt != null)
                {
                    SetOptionValue(lastOpt, arg);
                    lastOpt = null;
                }
                else
                {
                    if(defVal == null)
                    {
                        SetOptionValue(i.ToString(), arg);
                    }
                    else
                    {
                        if(defVal.Length > 0)
                        {
                            defVal += ArgSeparator;
                        }
                        defVal += arg;
                    }
                    i++;
                }
            }
            itr.Dispose();
            if(defVal != null)
            {
                SetOptionValue(WildcardString, defVal);
            }
        }

        virtual public void Execute()
        {
            if(Delegate != null)
            {
                Delegate(this);
            }
        }
    }
}
