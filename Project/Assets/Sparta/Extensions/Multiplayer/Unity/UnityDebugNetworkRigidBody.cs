﻿using System;
using Jitter;
using Jitter.LinearMath;
using UnityEngine;
using SocialPoint.Pooling;

namespace SocialPoint.Multiplayer
{
    public class BaseUnityDebugNetworkRigidBody : IDebugDrawer
    {
        NetworkGameObject _object;

        public Color Color;

        void IDebugDrawer.DrawLine(JVector start, JVector end)
        {
            Debug.DrawLine(start.ToUnity(), end.ToUnity(), Color);
        }

        void IDebugDrawer.DrawPoint(JVector pos)
        {
        }

        void IDebugDrawer.DrawTriangle(JVector pos1, JVector pos2, JVector pos3)
        {
            ((IDebugDrawer)this).DrawLine(pos1, pos2);
            ((IDebugDrawer)this).DrawLine(pos2, pos3);
            ((IDebugDrawer)this).DrawLine(pos3, pos1);
        }
    }

    public class UnityDebugNetworkServerRigidBody : BaseUnityDebugNetworkRigidBody, INetworkBehaviour, IUnityDebugBehaviour
    {
        NetworkGameObject _object;

        void INetworkBehaviour.OnAwake()
        {
        }

        void INetworkBehaviour.OnStart()
        {
            
        }

        NetworkGameObject INetworkBehaviour.GameObject
        {
            set
            {
                _object = value;
            }
        }

        public UnityDebugNetworkServerRigidBody()
        {
            Color = Color.red;
        }

        void INetworkBehaviour.Update(float dt)
        {
        }

        void IUnityDebugBehaviour.OnDrawGizmos()
        {
            if(_object == null)
            {
                return;
            }
            var rigidBody = _object.GetBehaviour<NetworkRigidBody>();
            if(rigidBody == null)
            {
                return;
            }
            rigidBody.EnableDebugDraw = true;
            rigidBody.DebugDraw(this);
        }

        void INetworkBehaviour.OnDestroy()
        {
        }

        public object Clone()
        {
            return ObjectPool.Get<UnityDebugNetworkServerRigidBody>();
        }

        public void Dispose()
        {
            ObjectPool.Return(this);
        }
    }

    public class UnityDebugNetworkClientRigidBody : BaseUnityDebugNetworkRigidBody, INetworkBehaviour, IUnityDebugBehaviour
    {
        NetworkServerSceneController _server;
        NetworkClientSceneController _client;
        NetworkGameObject _object;

        public UnityDebugNetworkClientRigidBody Init(NetworkServerSceneController server, NetworkClientSceneController client)
        {
            if(server == null)
            {
                throw new ArgumentNullException("server");
            }
            if(client == null)
            {
                throw new ArgumentNullException("client");
            }
            Color = Color.green;
            _server = server;
            _client = client;

            return this;
        }

        void INetworkBehaviour.OnAwake()
        {
        }

        void INetworkBehaviour.OnStart()
        {

        }

        NetworkGameObject INetworkBehaviour.GameObject
        {
            set
            {
                _object = value;
            }
        }

        void INetworkBehaviour.Update(float dt)
        {
        }

        void IUnityDebugBehaviour.OnDrawGizmos()
        {
            var scene = _server.GetSceneForTimestamp(_client.ServerTimestamp);
            if(scene == null)
            {
                return;
            }
            var oldObject = scene.FindObject(_object.Id);
            if(oldObject == null)
            {
                return;
            }
            var rigidBody = oldObject.GetBehaviour<NetworkRigidBody>();
            if(rigidBody == null)
            {
                return;
            }
            rigidBody.EnableDebugDraw = true;
            rigidBody.DebugDraw(this);
        }

        void INetworkBehaviour.OnDestroy()
        {
        }

        public object Clone()
        {
            return ObjectPool.Get<UnityDebugNetworkClientRigidBody>().Init(_server, _client);
        }

        public void Dispose()
        {
            ObjectPool.Return(this);
        }
    }
}
