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
                foreach(var opt in _options)
                {
                    foreach(var name in opt.Names)
                    {
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
            foreach(var opt in opts)
            {
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
                foreach(var opt in _options)
                {
                    foreach(var optName in opt.Names)
                    {
                        int i = MatchRepeat(name, optName);
                        if(i > 0)
                        {
                            opt.Value = i.ToString();
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
            foreach(var opt in _options)
            {
                var parts = new List<string>(opt.Names);
                if(parts.Contains(WildcardString))
                {
                    defVal = string.Empty;
                    break;
                }
            }

            foreach(var arg in args)
            {
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
