using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Multiplayer
{
    public class NetworkActionHandler
    {
        NetworkSceneActionHandler _actionHandler;
        TypedWriteSerializer _actionSerializer;
        TypedReadParser _actionParser;

        NetworkScene _scene;
        INetworkMessageSender _messageSender;

        public NetworkActionHandler(NetworkScene scene, INetworkMessageSender messageSender)
        {
            _actionHandler = new NetworkSceneActionHandler();
            _actionSerializer = new TypedWriteSerializer();
            _actionParser = new TypedReadParser();
            _scene = scene;
            _messageSender = messageSender;
        }

        public bool ApplyActionReceived(NetworkMessageData data, IReader reader)
        {
            var sceneMemento = new NetworkSceneMemento(_scene);
            return ApplyActionReceived(data, sceneMemento, reader);
        }

        public bool ApplyActionReceived(NetworkMessageData data, NetworkScene mementoScene, float mementoDelta, float mementoThreshold, byte clientId, IReader reader)
        {
            var sceneMemento = new NetworkSceneMemento(_scene, mementoScene, mementoDelta, mementoThreshold, clientId);
            return ApplyActionReceived(data, sceneMemento, reader);
        }

        bool ApplyActionReceived(NetworkMessageData data, NetworkSceneMemento sceneMemento, IReader reader)
        {
            return ApplyActionReceived(data.MessageType, sceneMemento, reader);
        }

        bool ApplyActionReceived(byte messageType, NetworkSceneMemento sceneMemento, IReader reader)
        {
            bool handled = false;
            object action;
            if(_actionParser.TryParseRaw(messageType, reader, out action))
            {
                handled = ApplyAction(action, sceneMemento);
            }
            return handled;
        }

        public bool ApplyActionAndSend(object action, bool unreliable = false)
        {
            bool handled = ApplyAction(action);
            var sent = SendAction(action, unreliable);
            return handled && sent;
        }

        public bool SendAction(object action, bool unreliable = false)
        {
            byte msgType;
            if(_actionSerializer.FindCode(action, out msgType))
            {
                var msg = _messageSender.CreateMessage(new NetworkMessageData {
                    MessageType = msgType,
                    Unreliable = unreliable
                });
                _actionSerializer.SerializeRaw(action, msg.Writer);
                msg.Send();
                return true;
            }
            else
            {
                Log.d("Code not found for action: " + action);
            }
            return false;
        }

        public void SerializeAction(object action, IWriter writer)
        {
            _actionSerializer.Serialize(action, writer);
        }

        public bool ApplyAction(object action)
        {
            return ApplyAction(action, new NetworkSceneMemento(_scene));
        }

        bool ApplyAction(object action, NetworkSceneMemento sceneMemento)
        {
            return _actionHandler.HandleAction(sceneMemento, action);
        }

        public void RegisterAction<T>(byte msgType, Action<NetworkSceneMemento, T> callback = null) where T : INetworkShareable, new()
        {
            if(callback != null)
            {
                _actionHandler.Register(callback);
            }
            _actionParser.Register<T>(msgType);
            _actionSerializer.Register<T>(msgType);
        }

        public void RegisterAction<T>(byte msgType, IActionHandler<NetworkSceneMemento, T> handler) where T : INetworkShareable, new()
        {
            _actionHandler.Register(handler);
            _actionParser.Register<T>(msgType);
            _actionSerializer.Register<T>(msgType);
        }

        public void UnregisterAction<T>()
        {
            _actionHandler.Unregister<T>();
            _actionParser.Unregister<T>();
            _actionSerializer.Unregister<T>();
        }
    }
}
