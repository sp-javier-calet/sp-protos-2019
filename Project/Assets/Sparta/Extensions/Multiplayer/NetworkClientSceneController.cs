using System;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class NetworkClientSceneController : INetworkClientDelegate, IDisposable
    {
        INetworkClient _client;
        NetworkGameScene _scene;
        IParser<NetworkGameScene> _sceneParser;
        IParser<InstantiateNetworkGameObjectEvent> _instParser;
        IParser<DestroyNetworkGameObjectEvent> _destParser;

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _sceneParser = new NetworkGameSceneParser();
            _instParser = new InstantiateNetworkGameObjectEventParser();
            _destParser = new DestroyNetworkGameObjectEventParser();
        }

        public void Dispose()
        {
            _client.RemoveDelegate(this);
        }

        public bool Equals(NetworkGameScene scene)
        {
            return _scene == scene;
        }

        void INetworkClientDelegate.OnConnected()
        {
            _scene = new NetworkGameScene();
        }

        void INetworkClientDelegate.OnDisconnected()
        {
            _scene = null;
        }

        void INetworkClientDelegate.OnMessageReceived(ReceivedNetworkMessage msg)
        {
            if(msg.MessageType == MsgType.UpdateSceneEvent)
            {
                if(_scene == null)
                {
                    _scene = _sceneParser.Parse(msg.Reader);
                }
                else
                {
                    _scene = _sceneParser.Parse(_scene, msg.Reader);
                }
            }
            else if(msg.MessageType == MsgType.InstantiateObjectEvent)
            {
                var ev = _instParser.Parse(msg.Reader);
                var go = new NetworkGameObject(ev.ObjectId, ev.Transform);
                _scene.AddObject(go);
                InstantiateObjectView(ev.PrefabName, go);
            }
            else if(msg.MessageType == MsgType.DestroyObjectEvent)
            {
                var ev = _destParser.Parse(msg.Reader);
                _scene.RemoveObject(ev.ObjectId);
                DestroyObjectView(ev.ObjectId);
            }
        }

        virtual protected void InstantiateObjectView(string prefabName, NetworkGameObject go)
        {
        }

        virtual protected void DestroyObjectView(int objectId)
        {
        }

        void INetworkClientDelegate.OnError(SocialPoint.Base.Error err)
        {
        }

    }
}