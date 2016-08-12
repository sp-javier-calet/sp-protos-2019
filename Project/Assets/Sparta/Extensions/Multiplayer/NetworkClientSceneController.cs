using System;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public class NetworkClientSceneController : INetworkClientDelegate, INetworkMessageReceiver, IDisposable
    {
        INetworkClient _client;
        NetworkScene _scene;
        IParser<NetworkScene> _sceneParser;
        IParser<InstantiateNetworkGameObjectEvent> _instParser;
        IParser<DestroyNetworkGameObjectEvent> _destParser;
        INetworkMessageReceiver _receiver;

        public NetworkClientSceneController(INetworkClient client)
        {
            _client = client;
            _client.AddDelegate(this);
            _client.RegisterReceiver(this);
            _sceneParser = new NetworkGameSceneParser();
            _instParser = new InstantiateNetworkGameObjectEventParser();
            _destParser = new DestroyNetworkGameObjectEventParser();
        }

        public void Dispose()
        {
            _client.RemoveDelegate(this);
            _client.RegisterReceiver(null);
        }

        public bool Equals(NetworkScene scene)
        {
            return _scene == scene;
        }

        void INetworkClientDelegate.OnConnected()
        {
            _scene = new NetworkScene();
        }

        void INetworkClientDelegate.OnDisconnected()
        {
            _scene = null;
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            if(data.MessageType == MsgType.UpdateSceneEvent)
            {
                if(_scene == null)
                {
                    _scene = _sceneParser.Parse(reader);
                }
                else
                {
                    _scene = _sceneParser.Parse(_scene, reader);
                }
            }
            else if(data.MessageType == MsgType.InstantiateObjectEvent)
            {
                var ev = _instParser.Parse(reader);
                InstantiateObjectView(ev);
            }
            else if(data.MessageType == MsgType.DestroyObjectEvent)
            {
                var ev = _destParser.Parse(reader);
                DestroyObjectView(ev);
            }
            else
            {
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
            }
        }

        virtual protected void InstantiateObjectView(InstantiateNetworkGameObjectEvent ev)
        {
        }

        virtual protected void DestroyObjectView(DestroyNetworkGameObjectEvent ev)
        {
        }

        void INetworkClientDelegate.OnError(SocialPoint.Base.Error err)
        {
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

    }
}