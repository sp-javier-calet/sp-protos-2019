
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkSceneActionHandler<T> : IActionHandler<NetworkScene, T>
    {
    }

    public interface INetworkSceneAction : IAppliable<NetworkScene>
    {
    }

    public class NetworkSceneActionHandler : ActionHandler<NetworkScene>
    {
    }
}
