using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Physics;

namespace SocialPoint.Multiplayer
{
    public class UnityDebugSceneMonoBehaviour : MonoBehaviour
    {
        public NetworkClientSceneController Client;
        public NetworkServerSceneController Server;

        public NetworkScene ClientScene
        {
            get
            {
                if(Client == null)
                {
                    return null;
                }
                return Client.Scene;
            }
        }

        public NetworkScene ServerScene
        {
            get
            {
                if(Server == null)
                {
                    return null;
                }
                return Server.Scene;
            }
        }
    }
}