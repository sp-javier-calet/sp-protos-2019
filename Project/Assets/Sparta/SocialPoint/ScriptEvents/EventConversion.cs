using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{

    public interface IScriptEventSerializer : ISerializer<object>
    {
        string Name { get; }
    }

    public interface IScriptEventParser : IParser<object>
    {
        string Name { get; }
    }

    public interface IScriptEventConverter : IScriptEventSerializer, IScriptEventParser
    {
    }

    public abstract class BaseScriptEventSerializer<T> : IScriptEventSerializer
    {
        public string Name { get; private set; }

        public BaseScriptEventSerializer(string name)
        {
            Name = name;
        }

        public Attr Serialize(object ev)
        {
            if(ev is T)
            {
                return SerializeEvent((T)ev);
            }
            return null;
        }

        abstract protected Attr SerializeEvent(T ev);
    }

    public abstract class BaseScriptEventParser<T> : IScriptEventParser
    {
        public string Name { get; private set; }

        public BaseScriptEventParser(string name)
        {
            Name = name;
        }

        public object Parse(Attr data)
        {
            return ParseEvent(data);
        }

        abstract protected T ParseEvent(Attr data);
    }

    public abstract class BaseScriptEventConverter<T> : IScriptEventConverter
    {
        public string Name { get; private set; }

        public BaseScriptEventConverter(string name)
        {
            Name = name;
        }

        public object Parse(Attr data)
        {
            return ParseEvent(data);
        }

        abstract protected T ParseEvent(Attr data);

        public Attr Serialize(object ev)
        {
            if(ev is T)
            {
                return SerializeEvent((T)ev);
            }
            return null;
        }

        abstract protected Attr SerializeEvent(T ev);
    }

    public class ScriptEventSerializer<T> : BaseScriptEventSerializer<T>
    {
        ISerializer<T> _serializer;

        public ScriptEventSerializer(string name, ISerializer<T> serializer = null) : base(name)
        {
            _serializer = serializer;
        }

        override protected Attr SerializeEvent(T ev)
        {
            
            if(_serializer != null)
            {
                return _serializer.Serialize((T)ev);
            }
            else
            {
                return new AttrEmpty();
            }
        }
    }

    public class ScriptEventParser<T> : BaseScriptEventParser<T>
    {
        IParser<T> _parser;

        public ScriptEventParser(string name, IParser<T> parser = null) : base(name)
        {
            _parser = parser;
        }

        override protected T ParseEvent(Attr data)
        {
            if(_parser != null)
            {
                return _parser.Parse(data);
            }
            else
            {
                return default(T);
            }
        }
    }

    public class ScriptEventConverter<T> : BaseScriptEventConverter<T>
    {
        ISerializer<T> _serializer;
        IParser<T> _parser;

        public ScriptEventConverter(string name, IParser<T> parser = null, ISerializer<T> serializer = null) : base(name)
        {
            _serializer = serializer;
            _parser = parser;
        }

        override protected T ParseEvent(Attr data)
        {
            if(_parser != null)
            {
                return _parser.Parse(data);
            }
            else
            {
                return default(T);
            }
        }

        override protected Attr SerializeEvent(T ev)
        {

            if(_serializer != null)
            {
                return _serializer.Serialize((T)ev);
            }
            else
            {
                return new AttrEmpty();
            }
        }
    }
}