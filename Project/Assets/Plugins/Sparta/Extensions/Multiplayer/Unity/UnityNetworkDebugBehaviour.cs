using UnityEngine;
using SocialPoint.Physics;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public interface IUnityDebugBehaviour
    {
        void OnDrawGizmos();
    }

    public class UnityNetworkClientDebugBehaviour : NetworkBehaviour
    {
        public NetworkGameObject ClientObject{ get { return GameObject; } }

        public NetworkClientSceneController Client{ get; private set; }

        public NetworkServerSceneController Server{ get; private set; }

        public NetworkGameObject ServerObject
        {
            get
            {
                if(Server == null)
                {
                    return null;
                }
                if(ClientObject == null)
                {
                    return null;
                }
                return Server.FindObject(ClientObject.Id);
            }
        }

        public UnityNetworkClientDebugBehaviour Init(NetworkClientSceneController client, NetworkServerSceneController server = null)
        {
            Client = client;
            Server = server;
            return this;
        }

        protected override void OnStart()
        {
            var viewBehavior = GameObject.GetBehaviour<UnityViewBehaviour>();
            if (viewBehavior != null)
            {
                var view = viewBehavior.View;
                var comp = view.GetComponent<UnityDebugMonoBehaviour>();
                if(comp == null)
                {
                    comp = view.AddComponent<UnityDebugMonoBehaviour>();
                }
                comp.NetworkBehaviour = this;
            }
        }

        public override object Clone()
        {
            var b = GameObject.Context.Pool.Get<UnityNetworkClientDebugBehaviour>();
            b.Init(Client, Server);
            return b;
        }
    }

    public class UnityDebugMonoBehaviour : MonoBehaviour
    {
        List<IUnityDebugBehaviour> _behaviours = new List<IUnityDebugBehaviour>(FinderSettings.DefaultListCapacity);
        public UnityNetworkClientDebugBehaviour NetworkBehaviour;

        GameObject _clientObject;
        GameObject _serverObject;

        public UnityEngine.Transform ClientTransform
        {
            get
            {
                if(NetworkBehaviour.Client == null)
                {
                    return null;
                }
                return _clientObject.transform;
            }
        }

        public UnityEngine.Transform ServerTransform
        {
            get
            {
                if(NetworkBehaviour.Server == null)
                {
                    return null;
                }
                return _serverObject.transform;
            }
        }

        public NetworkClientSceneController Client
        {
            get
            {
                if(NetworkBehaviour == null)
                {
                    return null;
                }
                return NetworkBehaviour.Client;
            }
        }

        public NetworkServerSceneController Server
        {
            get
            {
                if(NetworkBehaviour == null)
                {
                    return null;
                }
                return NetworkBehaviour.Server;
            }
        }

        public NetworkGameObject ClientObject
        {
            get
            {
                if(NetworkBehaviour == null)
                {
                    return null;
                }
                return NetworkBehaviour.ClientObject;
            }
        }

        public NetworkGameObject ServerObject
        {
            get
            {
                if(NetworkBehaviour == null)
                {
                    return null;
                }
                return NetworkBehaviour.ServerObject;
            }
        }

        void Start()
        {
            _clientObject = new GameObject("Client");
            _clientObject.SetActive(false);
            _clientObject.transform.parent = transform;
            _serverObject = new GameObject("Server");
            _serverObject.SetActive(false);
            _serverObject.transform.parent = transform;
        }

        void Update()
        {
            var ngo = ClientObject;
            if(ngo != null && _clientObject != null)
            {
                UpdateUnityTransform(ngo.Transform, _clientObject.transform);
            }
            ngo = ServerObject;
            if(ngo != null && _serverObject != null)
            {
                UpdateUnityTransform(ngo.Transform, _serverObject.transform);
            }
        }

        void OnDrawGizmos()
        {
            OnDrawGizmos(ClientObject);
            OnDrawGizmos(ServerObject);
        }

        void OnDrawGizmos(NetworkGameObject ngo)
        {
            if(ngo == null)
            {
                return;
            }
            ngo.GetBehaviours<IUnityDebugBehaviour>(_behaviours);
            for(var i = 0; i < _behaviours.Count; i++)
            {
                _behaviours[i].OnDrawGizmos();
            }
        }

        void OnDestroy()
        {
            GameObject.Destroy(_clientObject);
            GameObject.Destroy(_serverObject);
        }

        void UpdateUnityTransform(Transform t, UnityEngine.Transform ut)
        {
            ut.position = t.Position.ToUnity();
            ut.rotation = t.Rotation.ToUnity();
            ut.localScale = t.Scale.ToUnity();
        }
    }
}
