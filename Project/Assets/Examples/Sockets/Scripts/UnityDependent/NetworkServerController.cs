//-----------------------------------------------------------------------
// NetworkServerController.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using SocialPoint.Base;

namespace SocialPoint.Examples.Sockets
{
    public class NetworkServerController : MonoBehaviour
    {
        [SerializeField]
        Text _logText;

        INetworkServer _netServer;

        NetworkMatchDelegateFactory _matchDelegateFactory;

        #pragma warning disable 0414
        MultiMatchController _multiMatch;
        #pragma warning restore 0414


        void Awake()
        {
            InitServer();

            InitMatches();
        }

        void InitServer()
        {
            OnPrintLog("Create INetworkServer");
            var factory = Services.Instance.Resolve<INetworkServerFactory>();
            _netServer = factory.Create();
        }

        void InitMatches()
        {
            _matchDelegateFactory = new NetworkMatchDelegateFactory();
            _multiMatch = new MultiMatchController(_netServer, _matchDelegateFactory, TypeMessages.ConnectMessageType);
        }

        void Start()
        {
            StartServer();
        }

        void StartServer()
        {
            OnPrintLog("Start INetworkServer");
            _netServer.Start();
        }

        void OnApplicationQuit()
        {
            StopServer();
        }

        void StopServer()
        {
            OnPrintLog("Stop INetworkServer");
            _netServer.Stop();

            var dserver = _netServer as IDisposable;
            if(dserver != null)
            {
                dserver.Dispose();
            }
        }
        public void OnPrintLog(string message)
        {
            if(!IsHeadless())
            {
                string msg = "SERVER: " + message + "\n";
                Log.d(msg);
                _logText.text += msg;
            }
        }

        bool IsHeadless()
        {
            return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }
    }
}
