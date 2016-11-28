using System;
using System.Collections.Generic;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.IO;
using System.IO;
using System.Collections;

namespace Photon.Hive.Plugin.Authoritative
{
    /**
     * more info:
     * https://doc.photonengine.com/en/onpremise/current/plugins/manual
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-faq
     * https://doc.photonengine.com/en/onpremise/current/plugins/plugins-upload-guide
     */
    public class AuthoritativePlugin : NetworkServerPlugin
    {
        const byte IsOpenKey = 253;
        const int NoRandomMatchFoundCode = 32760;

        public override string Name
        {
            get
            {
                return "Authoritative";
            }
        }

        override protected int UpdateInterval
        {
            get
            {
                return _updateInterval;
            }
        }

        protected override bool Full
        {
            get
            {
                return _gameServer.Full;
            }
        }

        protected override int MaxPlayers
        {
            get
            {
                return _gameServer.MaxPlayers;
            }
        }

        NetworkServerSceneController _netServer;
        GameMultiplayerServerBehaviour _gameServer;
        int _lastUpdateTimestamp = 0;
        int _updateInterval = 100;
        string _navMeshFileLocation = "\\..\\data\\test_navmesh";

        public AuthoritativePlugin():base()
        {
            _netServer = new NetworkServerSceneController(this);
            _gameServer = new GameMultiplayerServerBehaviour(this, _netServer);
        }

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if (!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            string navmeshPath = typeof(AuthoritativePlugin).Assembly.Location + _navMeshFileLocation;
            if (!_gameServer.LoadNavMesh(navmeshPath, out errorMsg))
            {
                errorMsg = "Error loading NavMesh: " + errorMsg;
                return false;
            }

            return true;
        }

        protected override void Update()
        {
            float deltaTime = UpdateDeltaTime();
            _netServer.Update(deltaTime);
        }

        float UpdateDeltaTime()
        {
            int currentTimestamp = ((INetworkServer)this).GetTimestamp();
            float deltaTime = ((float)(currentTimestamp - _lastUpdateTimestamp)) * 0.001f;//Milliseconds to seconds
            _lastUpdateTimestamp = currentTimestamp;
            return deltaTime;
        }

    }
}
