using System;
using System.Linq;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.ScriptEvents
{
    public interface IScriptEventDispatcher : IDisposable
    {
        void AddListener(string name, Action<Attr> listener);
        void AddListener(Action<string, Attr> listener);
        bool RemoveListener(Action<Attr> listener);
        void RemoveListener(Action<string, Attr> listener);
        void AddConverter(IScriptEventConverter config);
        void Raise(string name, Attr args);
    }

    public interface IScriptEventsBridge : IDisposable
    {
        void Load(IScriptEventDispatcher dispatcher);
    }

    public interface IScriptEventConverter : ISerializer<object>, IParser<object>
    {
        string Name { get; }
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

    public class ScriptEventConverter<T> : BaseScriptEventConverter<T>
    {
        ISerializer<T> _serializer;
        IParser<T> _parser;

        public ScriptEventConverter(string name, IParser<T> parser=null, ISerializer<T> serializer=null): base(name)
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


    public class ScriptEventDispatcher : IScriptEventDispatcher
    {
        readonly Dictionary<string, List<Action<Attr>>> _delegates = new Dictionary<string, List<Action<Attr>>>();
        readonly List<Action<string, Attr>> _defaultDelegates = new List<Action<string, Attr>>();
        readonly List<IScriptEventConverter> _converters = new List<IScriptEventConverter>();
        readonly List<IScriptEventsBridge> _bridges = new List<IScriptEventsBridge>();
        IEventDispatcher _dispatcher;

        public ScriptEventDispatcher(IEventDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            _dispatcher.AddDefaultListener(OnRaised);
        }

        public void Dispose()
        {
            _dispatcher.RemoveDefaultListener(OnRaised);
            foreach(var bridge in _bridges)
            {
                bridge.Dispose();
            }
            Clear();
        }
        
        public void Clear()
        {
            _delegates.Clear();
            _defaultDelegates.Clear();
            _converters.Clear();
            _bridges.Clear();
        }

        public void AddBridges(IEnumerable<IScriptEventsBridge> bridges)
        {
            foreach(var bridge in bridges)
            {
                AddBridge(bridge);
            }
        }

        public void AddBridge(IScriptEventsBridge bridge)
        {
            if(bridge != null && !_bridges.Contains(bridge))
            {
                bridge.Load(this);
                _bridges.Add(bridge);
            }
        }

        public void AddListener(string name, Action<Attr> listener)
        {
            List<Action<Attr>> d;
            if(!_delegates.TryGetValue(name, out d))
            {
                d = new List<Action<Attr>>();
                _delegates[name] = d;
            }
            d.Add(listener);
        }

        public void AddListener(Action<string, Attr> listener)
        {
            if(!_defaultDelegates.Contains(listener))
            {
                _defaultDelegates.Add(listener);
            }
        }

        public bool RemoveListener(Action<Attr> listener)
        {
            bool found = false;
            foreach(var kvp in _delegates)
            {
                if(kvp.Value.Remove(listener))
                {
                    found = true;
                }
            }
            return found;
        }
        
        public void RemoveListener(Action<string, Attr> listener)
        {
            _defaultDelegates.Remove(listener);
        }

        public void AddConverter(IScriptEventConverter serializer)
        {
            if(!_converters.Contains(serializer))
            {
                _converters.Add(serializer);
            }
        }

        public void Raise(string name, Attr args)
        {
            var converter = _converters.FirstOrDefault(c => c.Name == name);
            if(converter != null)
            {
                var ev = converter.Parse(args);
                _dispatcher.Raise(ev);
            }
        }

        public void OnRaised(object ev)
        {
            Attr data = null;
            string name = null;
            foreach(var converter in _converters)
            {
                if(converter != null)
                {
                    data = converter.Serialize(ev);
                    if(data != null)
                    {
                        name = converter.Name;
                        break;
                    }
                }
            }
            if(data == null || string.IsNullOrEmpty(name))
            {
                return;
            }

            // default script delegates
            var sddlgList = new List<Action<string, Attr>>(_defaultDelegates);
            foreach(var dlg in sddlgList)
            {
                if(dlg != null)
                {
                    dlg(name, data);
                }
            }
            // script delegates
            List<Action<Attr>> sdlgList;
            if(_delegates.TryGetValue(name, out sdlgList))
            {
                foreach(var dlg in sdlgList)
                {
                    if(dlg != null)
                    {
                        dlg(data);                            
                    }
                }
            }
        }

    }
}