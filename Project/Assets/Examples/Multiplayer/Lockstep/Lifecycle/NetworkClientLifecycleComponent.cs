using System.Collections;
using SocialPoint.Base;
using SocialPoint.Lifecycle;
using SocialPoint.Network;

namespace Examples.Multiplayer.Lockstep
{
    public class NetworkClientLifecycleComponent : INetworkClientDelegate, ISetupComponent, ITeardownComponent, ICleanupComponent, IErrorDispatcher
    {
        public readonly INetworkClient NetworkClient;

        public NetworkClientLifecycleComponent(INetworkClient networkClient)
        {
            NetworkClient = networkClient;
            NetworkClient.AddDelegate(this);
        }

        void ICleanupComponent.Cleanup()
        {
            NetworkClient.RemoveDelegate(this);
            NetworkClient.Dispose();
        }

        IErrorHandler IErrorDispatcher.Handler { get; set; }

        void INetworkClientDelegate.OnClientRegistered(INetworkClient client)
        {
            Log.i("NetworkClientLifecycleComponent - INetworkClientDelegate.OnClientRegistered");
        }

        void INetworkClientDelegate.OnClientConnected()
        {
            Log.i("NetworkClientLifecycleComponent - INetworkClientDelegate.OnClientConnected");
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            Log.i("NetworkClientLifecycleComponent - INetworkClientDelegate.OnClientDisconnected");
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            Log.i("NetworkClientLifecycleComponent - INetworkClientDelegate.OnMessageReceived");
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            Log.i("NetworkClientLifecycleComponent - INetworkClientDelegate.OnNetworkError");
            TriggerError(err);
        }

        IEnumerator ISetupComponent.Setup()
        {
            NetworkClient.Connect();
            while(!NetworkClient.Connected)
            {
                yield return null;
            }
        }

        IEnumerator ITeardownComponent.Teardown()
        {
            NetworkClient.Disconnect();
            while(NetworkClient.Connected)
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