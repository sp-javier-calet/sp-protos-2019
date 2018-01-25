using UnityEngine;
using SocialPoint.Network;
using SocialPoint.Crash;

namespace SocialPoint.Multiplayer
{
    public interface IUnityNetworkBehaviour
    {
        void OnStart(NetworkGameObject ngo, GameObject go);

        void Update(float dt);

        void OnDestroy();
    }

    public class UnityNetworkClientSceneController : NetworkClientSceneController
    {
        public UnityNetworkClientSceneController(INetworkClient client, NetworkSceneContext context, ICrashReporter crashReporter) : base(client, context, crashReporter)
        {
        }

        public GameObject FindObjectViewById(int id)
        {
            return Scene.FindObject(id).GetBehaviour<UnityViewBehaviour>().View;
        }

        protected override void OnError(SocialPoint.Base.Error err)
        {
            Debug.LogError(err.Msg);
        }
    }
}
