//-----------------------------------------------------------------------
// DefaultMessage.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.IO;

namespace SocialPoint.Examples.Sockets
{
    public sealed class DefaultMessage : INetworkShareable
    {
        public string Message { get; private set; }

        public DefaultMessage(string message = null)
        {
            Message = message;
        }

        public void Deserialize(IReader reader)
        {
            Message = reader.ReadString();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(Message);
        }
    }
}
