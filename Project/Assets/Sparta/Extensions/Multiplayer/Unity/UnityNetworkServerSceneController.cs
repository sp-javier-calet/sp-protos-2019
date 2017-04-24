using UnityEngine;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public class UnityNetworkServerSceneController : NetworkServerSceneController
    {
        IUpdateScheduler _scheduler;

        public UnityNetworkServerSceneController(INetworkServer server, IUpdateScheduler scheduler) : base(server)
        {
            _scheduler = scheduler;
        }

        protected override void OnServerStarted()
        {
            base.OnServerStarted();
            _scheduler.Add(this);
        }

        protected override void OnServerStopped()
        {
            base.OnServerStopped();
            _scheduler.Remove(this);
        }

        public void Update()
        {
            base.Update(Time.deltaTime);
        }
    }
}