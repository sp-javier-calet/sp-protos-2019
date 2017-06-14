using System;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public sealed class ConsoleCommandOption
    {
        public string Config;
        public string Description;
        public string DefaultValue;
        string _value;

        const char NameSeparatorChar = '|';
        const char ValueSeparatorChar = ',';
        const string TwoString = "2";
        const string FalseString = "false";

        public string Value
        {
            get
            {
                return _value ?? DefaultValue;
            }

            set
            {
                if(_value == null)
                {
                    _value = value;
                    return;
                }
                if(_value == string.Empty && string.IsNullOrEmpty(value))
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
                    if(int.TryParse(value, out j))
                    {
                        i += j;
                        _value = i.ToString();
                        return;
                    }
                }
                _value += ValueSeparatorChar + value;
            }
        }

        public string Name
        {
            get
            {
                var names = Names;
                return names.Length > 0 ? names[names.Length - 1] : null;
            }
        }

        public string[] Names
        {
            get
            {
                return Config.Split(new []{ NameSeparatorChar });
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
                return int.TryParse(Value, out i) ? i : 1;
            }
        }

        public bool BoolValue
        {
            get
            {
                if(Value == null || string.Equals(Value, FalseString, StringComparison.CurrentCultureIgnoreCase))
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
                return Value == null ? null : new List<string>(Value.Split(new [] {
                    ValueSeparatorChar
                }));
            }
        }

        public ConsoleCommandOption(string config)
        {
            Config = config;
        }

        public ConsoleCommandOption WithDescription(string desc)
        {
            Description = desc;
            return this;
        }

        public ConsoleCommandOption WithDefaultValue(string val)
        {
            DefaultValue = val;
            return this;
        }
    }
}