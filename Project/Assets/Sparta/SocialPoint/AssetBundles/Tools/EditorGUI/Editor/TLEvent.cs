using System;
using System.Reflection;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// Event fired programatically or by actions performed by user interaction.
    /// </summary>
    /// The TLEvent class is the class that communicates that some actions happened during user interaction or code events
    /// and connects those actions to asyncronous callbacks.
    /// TLEvent can also be connected to other TLEvent. This will make that all events connected will be fired inmediately after
    /// the original one(in the very same Update cycle).
    /// TLEvent can also be used with generic parameters. If so, the field that contains the value has a special nomenclature and accessibility:
    /// - the field must be named "_value_pos<index>" and must be private where index is the position(0 for first) of the parameter in the class definition
    public abstract class TLAbstractEvent
    {
        public static readonly BindingFlags EventValueFieldBindings = BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.GetField | BindingFlags.Instance;

        string _name;
        List<TLAbstractEvent> _connectedEvents;
        Dictionary<TLAbstractEvent, EventArgsMapping> _connectedEventArgsMap;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string name { get { return _name; } }
        /// <summary>
        /// Gets the connected events.
        /// </summary>
        public List<TLAbstractEvent> connectedEvents { get { return _connectedEvents; } }

        public TLAbstractEvent( string name )
        {
            _name = name;
            _connectedEvents = new List<TLAbstractEvent> ();
            _connectedEventArgsMap = new Dictionary<TLAbstractEvent, EventArgsMapping> ();
        }

        /// <summary>
        /// (Internal Use Only)Call the actions associated to this event.
        /// </summary>
        public abstract void doAction();
        /// <summary>
        /// (Internal Use Only)Propagate this event. This means that if the event has parameters that can be propagated to other connected events it will do so.
        /// </summary>
        public void propagate(TLAbstractEvent toOther)
        {
            //If this event has args to propagate
            EventArgsMapping argsMap;
            if(_connectedEventArgsMap.TryGetValue(toOther, out argsMap))
            {
                for(int i = 0; i < argsMap.positionMap.Length; ++i)
                {
                    var sendArgPos = argsMap.positionMap[i].Key;
                    var recvArgPos = argsMap.positionMap[i].Value;
                    object sendValue = argsMap.senderType.GetField(String.Format("_value_pos{0}",sendArgPos),EventValueFieldBindings).GetValue(this);
                    argsMap.receiverType.GetField(String.Format("_value_pos{0}",recvArgPos), EventValueFieldBindings).SetValue(toOther, sendValue);
                }
            }
        }

        /// <summary>
        /// Connect another event to this event.
        /// </summary>
        public void Connect( TLAbstractEvent e )
        {
            if (this != e && !ContainsConnectedEvent(e))
                _connectedEvents.Add( e );
        }

        /// <summary>
        /// Connect another event to this event mapping it's event parameters to the new event by position index.
        /// </summary>
        public void ConnectWithArguments( TLAbstractEvent e, KeyValuePair<int, int>[] positionMap=null )
        {
            if (this != e && !ContainsConnectedEvent(e))
            {
                if(positionMap == null)
                {
                    int argLen = this.GetType().GetGenericArguments().Length;
                    if(argLen != e.GetType().GetGenericArguments().Length)
                    {
                        throw new Exception(String.Format("ConnectWithArguments - Mismatch lenght of arguments, must specify an argument position map", name));
                    }
                    positionMap = new KeyValuePair<int, int>[argLen];
                    for(int i = 0; i < argLen; ++i)
                    {
                        positionMap[i] = new KeyValuePair<int, int> (i,i);
                    }
                }

                ValidateArgumentMapping(e, positionMap);

                //Validated OK
                _connectedEvents.Add(e);
                _connectedEventArgsMap.Add(e, new EventArgsMapping(positionMap, this.GetType(), e.GetType()));
            }
        }

        void ValidateArgumentMapping(TLAbstractEvent e, KeyValuePair<int, int>[] positionMap)
        {
            //Perform argument mapping validation on Connect
            var senderArgTypes = this.GetType().GetGenericArguments();
            if(senderArgTypes.Length == 0)
            {
                throw new Exception(String.Format("ConnectWithArguments - Used with SENDER event that has no arguments: '{0}'", name));
            }
            var receiverArgTypes = e.GetType().GetGenericArguments();
            if(receiverArgTypes.Length == 0)
            {
                throw new Exception(String.Format("ConnectWithArguments - Used with RECEIVER event that has no arguments: '{0}'", e.name));
            }
            
            HashSet<int> senderUsedArgPositions = new HashSet<int> ();
            HashSet<int> receiverUsedArgPositons = new HashSet<int> ();
            for(int i = 0; i < positionMap.Length; ++i)
            {
                int senderArgPosition = positionMap[i].Key;
                if(senderArgPosition >= senderArgTypes.Length)
                {
                    throw new Exception(String.Format("ConnectWithArguments - SENDER argument position exceeds number of arguments: {0}", senderArgPosition));
                }
                int receiverArgPosition = positionMap[i].Value;
                if(receiverArgPosition >= receiverArgTypes.Length)
                {
                    throw new Exception(String.Format("ConnectWithArguments - RECEIVER argument position exceeds number of arguments: {0}", receiverArgPosition));
                }
                
                Type senderArgType = senderArgTypes[senderArgPosition];
                if(senderUsedArgPositions.Contains(senderArgPosition))
                {
                    throw new Exception(String.Format("ConnectWithArguments - SENDER argument position is being used twice: {0},'{1}'", senderArgPosition, senderArgType.Name));
                }
                Type receiverArgType = receiverArgTypes[receiverArgPosition];
                if(receiverUsedArgPositons.Contains(receiverArgPosition))
                {
                    throw new Exception(String.Format("ConnectWithArguments - RECEIVER argument position is being used twice: {0},'{1}'", receiverArgPosition, receiverArgType.Name));
                }
                
                if(!receiverArgType.IsAssignableFrom(senderArgType))
                {
                    throw new Exception(String.Format("ConnectWithArguments - RECEIVER argument position type is not assignable: {0},'{1}' from type '{2}'", receiverArgPosition, receiverArgType.Name, senderArgType.Name));
                }
                
                senderUsedArgPositions.Add(senderArgPosition);
                receiverUsedArgPositons.Add(receiverArgPosition);
            }
        }

        /// <summary>
        /// Disconnect the specified TLevent from this event.
        /// </summary>
        public void Disconnect( TLAbstractEvent e )
        {
            if ( !_connectedEvents.Contains( e ) )
                _connectedEvents.Remove( e );
            if ( _connectedEventArgsMap.ContainsKey( e ) )
                _connectedEventArgsMap.Remove( e );
        }

        bool ContainsConnectedEvent( TLAbstractEvent e )
        {
            for (int i = 0; i < connectedEvents.Count; ++i) {
                if (connectedEvents[i] != e) {
                    if (connectedEvents[i].ContainsConnectedEvent (e))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Sends this event for the window.
        /// </summary>
        /// <param name="window">Window.</param>
        public void Send(TLWindow window)
        {
            window.eventManager.AddEvent(this);
        }

        /// <summary>
        /// Sends this event for the window and all it's registered windows
        /// </summary>
        /// <param name="window">Window.</param>
        public void SendGlobal(TLWindow window)
        {
            window.AddGlobalEvent(this);
        }
    }

    public struct EventArgsMapping
    {
        public KeyValuePair<int, int>[] positionMap;
        public Type senderType; //To reduce runtime checks on propagate
        public Type receiverType; //To reduce runtime checks on propagate

        public EventArgsMapping(KeyValuePair<int, int>[] _positionMap, Type _senderT, Type _receiverT)
        {
            positionMap = _positionMap;
            senderType = _senderT;
            receiverType = _receiverT;
        }

        /// <summary>
        /// put sender position1, receiver position1, ... , to return a constructed KeyValuePair array
        /// </summary>
        /// <param name="positions">Positions.</param>
        public static KeyValuePair<int, int>[] Map(params int[] positions)
        {
            if(positions.Length % 2 != 0)
            {
                throw new Exception("Incorrect use of EventArgsMapping.Map. arguments should be paired.");
            }

            var result = new KeyValuePair<int, int>[positions.Length / 2];
            for(int i = 0; i < result.Length; ++i)
            {
                result[i] = new KeyValuePair<int, int>(positions[i], positions[i+1]);
            }
            return result;
        }
    }

    abstract class TLActionAbstract
    {
    }

    class TLAction : TLActionAbstract
    {
        Action _action;

        public TLAction(Action action)
        {
            _action = action;
        }

        public bool doAction()
        {
            if (_action != null)
                _action();
            else
                return false;
            return true;
        }
    }

    class TLAction<T> : TLActionAbstract
    {
        Action<T> _action;
        
        public TLAction(Action<T> action)
        {
            _action = action;
        }
        
        public bool doAction(T param)
        {
            if (_action != null)
                _action(param);
            else
                return false;
            return true;
        }
    }

    class TLAction<T, U> : TLActionAbstract
    {
        Action<T, U> _action;
        
        public TLAction(Action<T, U> action)
        {
            _action = action;
        }
        
        public bool doAction(T param1, U param2)
        {
            if (_action != null)
                _action(param1, param2);
            else
                return false;
            return true;
        }
    }

    /// <summary>
    /// Event with no parameters.
    /// </summary>
    public class TLEvent : TLAbstractEvent
    {
        List<TLAction> _actions;

        public TLEvent( string name ) : base(name)
        {
            _actions = new List<TLAction> ();
        }

        public override void doAction()
        {
            int i = 0;
            while (i < _actions.Count) {
                if (_actions[i].doAction())
                    i++;
                else
                    _actions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Connect a parameterless action to this event.
        /// </summary>
        public void Connect( Action action )
        {
            _actions.Add(new TLAction(action));
        }

        /// <summary>
        /// Disconnect a parameterless action to this event.
        /// </summary>
        public void Disconnect( Action action )
        {
            _actions.Remove(new TLAction(action));
        }
    }

    /// <summary>
    /// Event with a single parameter. Can fire parameterless methods
    /// </summary>
    public class TLEvent<T> : TLAbstractEvent
    {
        List<TLActionAbstract> _actions;
        /// <summary>
        /// The value passed in the event.
        /// </summary>
        T _value_pos0;
        
        public TLEvent( string name ) : base(name)
        {
            _actions = new List<TLActionAbstract> ();
        }

        public TLEvent( string name, T value ) : base(name)
        {
            _actions = new List<TLActionAbstract> ();
            _value_pos0 = value;
        }
        
        public override void doAction()
        {
            int i = 0;
            while (i < _actions.Count) {
                bool result;
                var action = _actions[i] as TLAction<T>;
                if(action != null)
                {
                    result = action.doAction(_value_pos0);
                }
                else
                {
                    result = ((TLAction)_actions[i]).doAction();
                }
                if(result)
                    i++;
                else
                    _actions.RemoveAt(i);
            }
        }

        /// <summary>
        /// Connect an action with a single parameter to this event.
        /// </summary>
        public void Connect( Action<T> action )
        {
            _actions.Add(new TLAction<T>(action));
        }

        public void Connect( Action action )
        {
            _actions.Add(new TLAction(action));
        }

        /// <summary>
        /// Disconnect an action with a single parameter from this event.
        /// </summary>
        public void Disconnect( Action<T> action )
        {
            _actions.Remove(new TLAction<T>(action));
        }

        public void Disconnect( Action action )
        {
            _actions.Remove(new TLAction(action));
        }

        /// <summary>
        /// Sets the value of this event for passing to the actions when it is fired.
        /// </summary>
        public void SetValue(T value)
        {
            _value_pos0 = value;
        }

        public void Send(TLWindow window, T value)
        {
            SetValue(value);
            window.eventManager.AddEvent(this);
        }
        
        public void SendGlobal(TLWindow window, T value)
        {
            SetValue(value);
            window.AddGlobalEvent(this);
        }
    }

    /// <summary>
    /// Event with two(2) parameters. Can fire parameterless methods
    /// </summary>
    public class TLEvent<T, U> : TLAbstractEvent
    {
        List<TLActionAbstract> _actions;
        /// <summary>
        /// The first value passed in the event.
        /// </summary>
        T _value_pos0;
        /// <summary>
        /// The second value passed in the event.
        /// </summary>
        U _value_pos1;
        
        public TLEvent( string name ) : base(name)
        {
            _actions = new List<TLActionAbstract> ();
        }
        
        public TLEvent( string name, T value1, U value2 ) : base(name)
        {
            _actions = new List<TLActionAbstract> ();
            _value_pos0 = value1;
            _value_pos1 = value2;
        }
        
        public override void doAction()
        {
            int i = 0;
            while (i < _actions.Count) {
                bool result;
                var action = _actions[i] as TLAction<T, U>;
                if(action != null)
                {
                    result = action.doAction(_value_pos0, _value_pos1);
                }
                else
                {
                    result = ((TLAction)_actions[i]).doAction();
                }
                if(result)
                    i++;
                else
                    _actions.RemoveAt(i);
            }
        }
        
        /// <summary>
        /// Connect an action with two parameters to this event.
        /// </summary>
        public void Connect( Action<T, U> action )
        {
            _actions.Add(new TLAction<T, U>(action));
        }
        
        public void Connect( Action action )
        {
            _actions.Add(new TLAction(action));
        }
        
        /// <summary>
        /// Disconnect an action with two parameters from this event.
        /// </summary>
        public void Disconnect( Action<T, U> action )
        {
            _actions.Remove(new TLAction<T, U>(action));
        }
        
        public void Disconnect( Action action )
        {
            _actions.Remove(new TLAction(action));
        }
        
        /// <summary>
        /// Sets the value of this event for passing to the actions when it is fired.
        /// </summary>
        public void SetValue(T value1, U value2)
        {
            _value_pos0 = value1;
            _value_pos1 = value2;
        }

        public void Send(TLWindow window, T value1, U value2)
        {
            SetValue(value1, value2);
            window.eventManager.AddEvent(this);
        }
        
        public void SendGlobal(TLWindow window, T value1, U value2)
        {
            SetValue(value1, value2);
            window.AddGlobalEvent(this);
        }
    }
}
