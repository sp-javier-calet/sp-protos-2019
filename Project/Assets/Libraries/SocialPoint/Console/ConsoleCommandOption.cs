
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Console
{
    public class ConsoleCommandOption
    {
        public string Config;
        public string Description;
        public string DefaultValue;
        public bool StringValue = true;
        private string _value;

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
                _value = value;
            }
        }

        public string Name
        {
            get
            {
                var parts = Config.Split(new char[]{'|'});
                if(parts.Length > 0)
                {
                    return parts[parts.Length - 1];
                }
                return null;
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
                return int.Parse(Value);
            }
        }

        public bool BoolValue
        {
            get
            {
                if(Value == "yes" || Value == "true" || Value == "1")
                {
                    return true;
                }
                return false;
            }
        }
        
        public IList<string> ListValue
        {
            get
            {
                if(Value == null)
                {
                    return null;
                }
                return new List<string>(Value.Split(new char[]{','}));
            }
        }

        public ConsoleCommandOption(string config)
        {
            Config = config;
        }

        public ConsoleCommandOption withStringValue(bool enabled)
        {
            StringValue = enabled;
            return this;
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