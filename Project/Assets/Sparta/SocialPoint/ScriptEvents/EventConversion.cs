using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    public interface IScriptEventSerializer : IAttrObjSerializer<object>
    {
        string Name { get; }
    }

    public interface IScriptEventParser : IAttrObjParser<object>
    {
        string Name { get; }
    }

    public interface IScriptEventConverter : IScriptEventSerializer, IScriptEventParser
    {
    }

    public abstract class BaseScriptEventSerializer<T> : IScriptEventSerializer
    {
        public string Name { get; private set; }

        protected BaseScriptEventSerializer(string name)
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

        protected BaseScriptEventParser(string name)
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

        protected BaseScriptEventConverter(string name)
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

    public sealed class ScriptEventSerializer<T> : BaseScriptEventSerializer<T>
    {
        readonly IAttrObjSerializer<T> _serializer;

        public ScriptEventSerializer(string name, IAttrObjSerializer<T> serializer = null) : base(name)
        {
            _serializer = serializer;
        }

        override protected Attr SerializeEvent(T ev)
        {
            return _serializer != null ? _serializer.Serialize(ev) : new AttrEmpty();
        }
    }

    public sealed class ScriptEventParser<T> : BaseScriptEventParser<T>
    {
        readonly IAttrObjParser<T> _parser;

        public ScriptEventParser(string name, IAttrObjParser<T> parser = null) : base(name)
        {
            _parser = parser;
        }

        override protected T ParseEvent(Attr data)
        {
            return _parser != null ? _parser.Parse(data) : default(T);
        }
    }

    public sealed class ScriptEventConverter<T> : BaseScriptEventConverter<T>
    {
        readonly IAttrObjSerializer<T> _serializer;
        readonly IAttrObjParser<T> _parser;

        public ScriptEventConverter(string name, IAttrObjParser<T> parser = null, IAttrObjSerializer<T> serializer = null) : base(name)
        {
            _serializer = serializer;
            _parser = parser;
        }

        override protected T ParseEvent(Attr data)
        {
            return _parser != null ? _parser.Parse(data) : default(T);
        }

        override protected Attr SerializeEvent(T ev)
        {
            return _serializer != null ? _serializer.Serialize(ev) : new AttrEmpty();
        }
    }
}