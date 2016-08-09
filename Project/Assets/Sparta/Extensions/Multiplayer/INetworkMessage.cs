
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkMessage
    {
        IWriter Writer{ get; }
        void Send();
    }
}