using System;

namespace SocialPoint.Multiplayer
{
    public class NetworkClientSceneController : INetworkClientDelegate, IDisposable
    {
        INetworkClient _client;
        NetworkGameScene _scene;
        IParser<NetworkGameScene> _sceneParser;
        IParser<InstantiateEvent> _instParser;
        IParser<DestroyEvent> _destParser;

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _sceneParser = new NetworkGameSceneParser();
            _instParser = new InstantiateEventParser();
            _destParser = new DestroyEventParser();
        }

        public NetworkGameScene Scene
        {
            get
            {
                return _scene;
            }
        }

        public void Dispose()
        {
            _client.RemoveDelegate(this);
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
            if(msg.MessageType == NetworkGameScene.MessageType)
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
            else if(msg.MessageType == InstantiateEvent.MessageType)
            {
                var ev = _instParser.Parse(msg.Reader);
                // TODO: instantiate prefab
                _scene.AddObject(new NetworkGameObject(ev.ObjectId, ev.Transform));
            }
            else if(msg.MessageType == DestroyEvent.MessageType)
            {
                var ev = _destParser.Parse(msg.Reader);
                _scene.RemoveObject(ev.ObjectId); 
            }
        }

        void INetworkClientDelegate.OnError(SocialPoint.Base.Error err)
        {
        }

    }
}