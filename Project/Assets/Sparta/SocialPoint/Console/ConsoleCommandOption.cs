
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public sealed class ConsoleCommandOption
    {
        public string Config;
        public string Description;
        public string DefaultValue;
        private string _value;

        const char NameSeparatorChar = '|';
        const char ValueSeparatorChar = ',';
        const string TwoString = "2";
        const string FalseString = "false";

        public string Value
        {
            get
            {
                if(_value == null)
                {
                    return DefaultValue;
                }
                return _value;
            }

            set
            {
                if(_value == null)
                {
                    _value = value;
                    return;
                }
                else if(_value == string.Empty && string.IsNullOrEmpty(value))
                {
                    _value = TwoString;
                    return;
                }
                int i, j;
                if(int.TryParse(_value, out i))
                {
                    if(string.IsNullOrEmpty(value))
                    {
                        i++;
                        _value = i.ToString();
                        return;
                    }
                    else if(int.TryParse(value, out j))
                    {
                        i += j;
                        _value = i.ToString();
                        return;
                    }
                }
                _value += ValueSeparatorChar+value;
            }
        }

        public string Name
        {
            get
            {
                var names = Names;
                if(names.Length > 0)
                {
                    return names[names.Length - 1];
                }
                return null;
            }
        }

        public string[] Names
        {
            get
            {
                return Config.Split(new char[]{NameSeparatorChar});
            }
        }

        public int IntValue
        {
            get
            {
                if(Value == null)
                {
                    return 0;
                }
                int i;
                if(int.TryParse(Value, out i))
                {
                    return i;
                }
                return 1;
            }
        }

        public bool BoolValue
        {
            get
            {
                if(Value == null || Value.ToLower() == FalseString)
                {
                    return false;
                }
                return true;
            }
        }
        
        public List<string> ListValue
        {
            get
            {
                if(Value == null)
                {
                    return null;
                }
                return new List<string>(Value.Split(new char[]{ValueSeparatorChar}));
            }
        }

        public ConsoleCommandOption(string config)
        {
            Config = config;
        }

        public ConsoleCommandOption withDescription(string desc)
        {
            Description = desc;
            return this;
        }

        public ConsoleCommandOption withDefaultValue(string val)
        {
            DefaultValue = val;
            return this;
        }
    }
}