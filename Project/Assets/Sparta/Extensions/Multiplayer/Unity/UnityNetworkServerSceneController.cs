﻿using UnityEngine;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{

    public class UnityNetworkServerSceneController : NetworkServerSceneController, IUpdateable
    {
        IUpdateScheduler _scheduler;

        public UnityNetworkServerSceneController(INetworkServer server, IUpdateScheduler scheduler):base(server)
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