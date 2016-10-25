
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkSceneActionHandler<T> : SocialPoint.Utils.IActionHandler<NetworkScene, T>
    {
    }

    public interface INetworkSceneAction
    {
        void Apply(NetworkScene scene);
    }

    public class NetworkSceneActionHandler : INetworkSceneActionHandler<INetworkSceneAction>
    {
        public void HandleAction(NetworkScene scene, INetworkSceneAction action)
        {
            if(action != null)
            {
                action.Apply(scene);
            }
        }
    }
}
