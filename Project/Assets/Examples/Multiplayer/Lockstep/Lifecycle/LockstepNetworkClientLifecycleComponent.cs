//-----------------------------------------------------------------------
// LockstepNetworkClientLifecycleComponent.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System.Collections;
using SocialPoint.Attributes;
using SocialPoint.Lifecycle;
using SocialPoint.Lockstep;
using SocialPoint.Network;

namespace Examples.Multiplayer.Lockstep
{
    public class LockstepNetworkClientLifecycleComponent : ISetupComponent, ITeardownComponent, ICleanupComponent
    {
        readonly GameNetworkSceneController _sceneController;
        Attr _receivedResult;

        public LockstepNetworkClientLifecycleComponent(GameNetworkSceneController sceneController, INetworkClient networkClient)
        {
            _sceneController = sceneController;
            LockstepNetworkClient = new LockstepNetworkClient(networkClient, sceneController.Client, _sceneController.CommandFactory);
            LockstepNetworkClient.EndReceived += OnEndReceived;

            _sceneController.ClientFinished += LockstepNetworkClient.SendPlayerFinish;
        }

        public LockstepNetworkClient LockstepNetworkClient { get; private set; }

        void ICleanupComponent.Cleanup()
        {
            _sceneController.ClientFinished -= LockstepNetworkClient.SendPlayerFinish;
            LockstepNetworkClient.Dispose();
        }

        IEnumerator ISetupComponent.Setup()
        {
            LockstepNetworkClient.SendPlayerReady();
            while(!LockstepNetworkClient.Running)
            {
                yield return null;
            }
        }

        IEnumerator ITeardownComponent.Teardown()
        {
            _receivedResult = null;
            _sceneController.SendResult(LockstepNetworkClient);
            while(_receivedResult == null)
            {
                yield return null;
            }
        }

        void OnEndReceived(Attr data)
        {
            _receivedResult = data;
        }
    }
}