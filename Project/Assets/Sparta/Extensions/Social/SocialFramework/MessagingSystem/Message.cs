using System.Collections.Generic;

namespace SocialPoint.Social
{
    public sealed class Message
    {
        internal string Id{ get; private set; }

        readonly IMessageOrigin _origin;
        readonly IMessagePayload _payload;
        readonly HashSet<string> _properties;

        public Message(string id, IMessageOrigin origin, IMessagePayload payload)
        {
            Id = id;
            _origin = origin;
            _payload = payload;
            _properties = new HashSet<string>();
        }

        public T Origin<T>() where T : class, IMessageOrigin
        {
            return _origin as T;
        }

        public T Payload<T>() where T : class, IMessagePayload
        {
            return _payload as T;
        }

        public bool HasProperty(string property)
        {
            return _properties.Contains(property);
        }

        public IEnumerator<string> GetProperties()
        {
            return _properties.GetEnumerator();
        }

        internal void AddProperty(string property)
        {
            _properties.Add(property);
        }

        internal void RemoveProperty(string property)
        {
            _properties.Remove(property);
        }
    }
}