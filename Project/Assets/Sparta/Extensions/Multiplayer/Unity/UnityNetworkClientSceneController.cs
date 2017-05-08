﻿using UnityEngine;
using SocialPoint.Network;

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
        public UnityNetworkClientSceneController(INetworkClient client) : base(client)
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
