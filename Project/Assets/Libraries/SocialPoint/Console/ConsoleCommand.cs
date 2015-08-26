
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public delegate void ConsoleCommandDelegate(ConsoleCommand cmd);

    public class ConsoleCommand : IEnumerable<KeyValuePair<string, ConsoleCommandOption>>
    {
        public ConsoleCommandDelegate Delegate = null;
        public string Description = null;
        private IDictionary<string, ConsoleCommandOption> _options = new Dictionary<string, ConsoleCommandOption>();

        public IEnumerator<KeyValuePair<string, ConsoleCommandOption>> GetEnumerator()
        {
            return _options.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ConsoleCommandOption this[string name]
        {
            get
            {
                return _options[name];
            }
            set
            {
                _options[name] = value;
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
            var name = opt.Name;
            if(name == null)
            {
                throw new ConsoleException("Invalid option name");
            }
            _options[name] = opt;
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

        private void SetOptionValue(string name, string value)
        {
            foreach(var pair in _options)
            {
                var parts = new List<string>(pair.Value.Config.Split(new char[]{'|'}));
                if(parts.Contains(name))
                {
                    pair.Value.Value = value;
                    return;
                }
            }
            throw new ConsoleException(string.Format("Could not find option '{0}'.", name));
        }

        public void SetOptionValues(IEnumerable<string> args)
        {
            string lastOpt = null;
            int i = 0;

            string defVal = null;
            foreach(var pair in _options)
            {
                var parts = new List<string>(pair.Value.Config.Split(new char[]{'|'}));
                if(parts.Contains("*"))
                {
                    defVal = "";
                    break;
                }
            }

            foreach(var arg in args)
            {
                if(arg.StartsWith("-"))
                {
                    lastOpt = null;
                    var opt = arg.Trim(new char[]{'-'});
                    var p = opt.IndexOf("=");
                    if(p == -1)
                    {
                        if(_options.ContainsKey(opt) && _options[opt].StringValue == false)
                        {
                            SetOptionValue(opt, "true");
                        }
                        else
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
                            defVal += " ";
                        }
                        defVal += arg;
                    }
                    i++;
                }
            }
            if(defVal != null)
            {
                SetOptionValue("*", defVal);
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
