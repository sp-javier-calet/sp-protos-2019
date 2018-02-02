using System;
using SocialPoint.Dependency;
using SocialPoint.Network;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using SocialPoint.Base;

namespace SocialPoint.Sockets
{
    public class NetworkServerController : MonoBehaviour
    {
        [SerializeField]
        Text _logText;

       INetworkServer _netServer;

        NetworkMatchDelegateFactory _matchDelegateFactory;

        MultiMatchController _multiMatch;

        void Awake()
        {
            InitServer();

            InitMatches();
        }

        void InitServer()
        {
            OnPrintLog("Create INetworkServer");
            _netServer = Services.Instance.Resolve<INetworkServer>();
        }

        void InitMatches()
        {
            _matchDelegateFactory = new NetworkMatchDelegateFactory();
            _multiMatch = new MultiMatchController(_netServer, _matchDelegateFactory, TypeMessages.ConnectMessageType);

            //HACK FOR ASSIGNED BUT NEVER USED VARIABLES ERROR!!!!!!!!
            _netServer.RemoveDelegate(_multiMatch);
            _netServer.AddDelegate(_multiMatch);
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
            if (dserver != null)
            {
                dserver.Dispose();
            }
        }
        public void OnPrintLog(string message)
        {
            if (!IsHeadless())
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
