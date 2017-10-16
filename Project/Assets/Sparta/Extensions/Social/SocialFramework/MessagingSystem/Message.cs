using System.Collections.Generic;
using System.Text;

namespace SocialPoint.Social
{
    public sealed class Message
    {
        public string Id{ get; private set; }

        public int Timestamp{ get; private set; }

        readonly IMessageOrigin _origin;
        readonly IMessagePayload _payload;
        readonly HashSet<string> _properties;

        public Message(string id, int timestamp, IMessageOrigin origin, IMessagePayload payload)
        {
            Id = id;
            Timestamp = timestamp;
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

        public void AddProperty(string property)
        {
            _properties.Add(property);
        }

        public void RemoveProperty(string property)
        {
            _properties.Remove(property);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("Ori: {0} - Payld: {1} - Prop: ", _origin.Identifier, _payload.Identifier);
            using(var itr = _properties.GetEnumerator())
            {
                while(itr.MoveNext())
                {
                    builder.Append(itr.Current);
                    builder.Append(", ");
                }
            }
            return builder.ToString();
        }
    }
}