//-----------------------------------------------------------------------
// NetworkServerLifecycleComponent.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections;
using SocialPoint.Base;
using SocialPoint.Lifecycle;
using SocialPoint.Network;

namespace Examples.Multiplayer.Lockstep
{
    public class NetworkServerLifecycleComponent : ISetupComponent, ITeardownComponent, ICleanupComponent, IErrorDispatcher, INetworkServerDelegate
    {
        public readonly INetworkServer NetworkServer;

        public NetworkServerLifecycleComponent(INetworkServer server)
        {
            NetworkServer = server;
            NetworkServer.AddDelegate(this);
        }

        void ICleanupComponent.Cleanup()
        {
            NetworkServer.RemoveDelegate(this);
            NetworkServer.Dispose();
        }

        IErrorHandler IErrorDispatcher.Handler { get; set; }

        void INetworkServerDelegate.OnServerStarted()
        {
            Log.i("NetworkServerLifecycleComponent - INetworkServerDelegate.OnServerStarted");
        }

        void INetworkServerDelegate.OnServerStopped()
        {
            Log.i("NetworkServerLifecycleComponent - INetworkServerDelegate.OnServerStopped");
        }

        void INetworkServerDelegate.OnClientConnected(byte clientId)
        {
            Log.i("NetworkServerLifecycleComponent - INetworkServerDelegate.OnClientConnected " + clientId);
        }

        void INetworkServerDelegate.OnClientDisconnected(byte clientId)
        {
            Log.i("NetworkServerLifecycleComponent - INetworkServerDelegate.OnClientDisconnected " + clientId);
        }

        void INetworkServerDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkServerDelegate.OnNetworkError(Error err)
        {
            Log.e("NetworkServerLifecycleComponent - INetworkServerDelegate.OnNetworkError " + err);
            TriggerError(err);
        }

        IEnumerator ISetupComponent.Setup()
        {
            NetworkServer.Start();
            while(!NetworkServer.Running)
            {
                yield return null;
            }
        }

        IEnumerator ITeardownComponent.Teardown()
        {
            NetworkServer.Stop();
            while(NetworkServer.Running)
            {
                yield return null;
            }
        }

        void TriggerError(Error err)
        {
            var handler = ((IErrorDispatcher) this).Handler;
            if(handler != null)
            {
                handler.OnError(err);
            }
        }
    }
}