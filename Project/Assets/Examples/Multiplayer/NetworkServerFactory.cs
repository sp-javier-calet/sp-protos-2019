using System;
using System.Reflection;
using System.Collections.Generic;
using SocialPoint.Multiplayer;
using SocialPoint.Network;

namespace Examples.Multiplayer
{
    public class NetworkServerFactory : INetworkServerGameFactory
    {
        string _navMeshFileLocation = "\\..\\data\\test_navmesh";

        public object Create(INetworkServer server, NetworkServerSceneController ctrl, Dictionary<string, string> config)
        {
            var gameServer = new GameMultiplayerServerBehaviour(server, ctrl); 
            string navmeshPath = Assembly.GetExecutingAssembly().Location + _navMeshFileLocation;
            string errorMsg;
            if (!gameServer.LoadNavMesh(navmeshPath, out errorMsg))
            {
                throw new InvalidOperationException("Error loading NavMesh: " + errorMsg);
            }
            return gameServer;
        }
    }
}