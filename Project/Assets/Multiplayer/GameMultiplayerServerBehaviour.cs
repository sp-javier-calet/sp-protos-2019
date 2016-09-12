using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using System;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.Math;

public class GameMultiplayerServerBehaviour : INetworkServerSceneReceiver, IDisposable
{
    INetworkServer _server;
    NetworkServerSceneController _controller;

    Dictionary<int,int> _updateTimes;
    float _moveInterval = 1.0f;
    float _timeSinceLastMove = 0.0f;
    int _maxUpdateTimes = 1;
    Vector3 _movement;

    public PhysicsWorld PhysicsWorld;
    public PhysicsWorldLateHelper PhysicsLateHelper;

    //*** TEST
    static NetworkGameObject playerCube = null;

    public GameMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl)
    {
        _server = server;
        _controller = ctrl;
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(MovementAction.Apply);
        _updateTimes = new Dictionary<int,int>();
        _movement = new Vector3(2.0f, 0.0f, 2.0f);

        AddPhysicsWorld();
    }

    public void Dispose()
    {
        _controller.RegisterReceiver(null);
    }

    void INetworkServerSceneBehaviour.Update(float dt, NetworkScene scene, NetworkScene oldScene)
    {
        _timeSinceLastMove += dt;

        if(_timeSinceLastMove > _moveInterval)
        {
            _timeSinceLastMove = 0.0f;
            var itr = _controller.Scene.GetObjectEnumerator();
            while(itr.MoveNext())
            {
                if(itr.Current == playerCube)
                {
                    //Using first cube as MovementAction target
                    continue;
                }

                var id = itr.Current.Id;
                var p = itr.Current.Transform.Position;

                p += new Vector3(
                    RandomUtils.Range(-_movement.X, _movement.X),
                    0.0f,//RandomUtils.Range(-_movement.Y, _movement.Y),
                    RandomUtils.Range(-_movement.Z, _movement.Z));

                _controller.Tween(id, p, _moveInterval);
                int times;

                if(!_updateTimes.TryGetValue(id, out times))
                {
                    times = 0;
                }
                if(times > _maxUpdateTimes)
                {
                    var go = _controller.Scene.FindObject(id);
                    if(go != null)
                    {
                        SendExplosionEvent(go.Transform);
                    }
                    _controller.Destroy(id);
                    _updateTimes.Remove(id);
                }
                else
                {
                    _updateTimes[id] = times + 1;
                }
            }
            itr.Dispose();
        }
    }

    void SendExplosionEvent(Transform t)
    {
        _server.SendMessage(new NetworkMessageData {
            MessageType = GameMsgType.ExplosionEvent
        }, new ExplosionEvent {
            Position = t.Position
        });
    }

    void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
    {
        if(data.MessageType == GameMsgType.ClickAction)
        {
            var ac = reader.Read<ClickAction>();
            if(playerCube != null && IntersectsRay(playerCube, ac.Ray))
            {
                UnityEngine.Debug.Log("*** TEST Ray Intersects Player!");
            }
            else
            {
                NetworkGameObject currentCube = _controller.Instantiate("Cube", new Transform(
                                                    ac.Position, Quaternion.Identity, Vector3.One));

                if(playerCube == null)
                {
                    playerCube = currentCube;
                }

                AddCollision(currentCube);
            }

        }
        else if(data.MessageType == GameMsgType.MovementAction)
        {
            var ac = reader.Read<MovementAction>();
            _controller.OnAction<MovementAction>(ac, data.ClientId);
        }
    }

    void INetworkServerSceneBehaviour.OnClientConnected(byte clientId)
    {
    }

    void INetworkServerSceneBehaviour.OnClientDisconnected(byte clientId)
    {
    }

    void AddPhysicsWorld()
    {
        PhysicsLateHelper = new PhysicsWorldLateHelper();
        PhysicsWorld = new PhysicsWorld(new UnityPhysicsDebugger(), PhysicsLateHelper);
        PhysicsWorld.DoDebugDraw = true;
        PhysicsWorld.Awake();
        _controller.AddBehaviour(PhysicsWorld);
    }


    void AddCollision(NetworkGameObject go)
    {
        PhysicsRigidBody RigidBody = new PhysicsRigidBody();
        RigidBody.collisionFlags = CollisionFlags.KinematicObject;

        //PhysicsCollisionObject = new PhysicsCollisionObject();
        RigidBody.NetworkGameObject = go;
        PhysicsBoxShape boxShape = new PhysicsBoxShape(new Vector3(0.5f));
        RigidBody.CollisionShape = boxShape;
        RigidBody.Debugger = new UnityPhysicsDebugger();//TODO: Share single debugger

        CollisionObject co = RigidBody.GetCollisionObject();
        //co.CollisionFlags = CollisionFlags.KinematicObject;
        co.ActivationState = ActivationState.DisableDeactivation;

        //PhysicsWorld.AddCollisionObject(go.PhysicsCollisionObject);
        RigidBody.PhysicsWorld = PhysicsWorld;
        //go.PhysicsCollisionObject.Start();//TODO: Change start to remove internal Add to world
        PhysicsDefaultCollisionCallbacks collCallback = new PhysicsDefaultCollisionCallbacks(co);
        RigidBody.AddOnCollisionCallbackEventHandler(collCallback);

        _controller.AddRigidbody(go.Id, RigidBody);
    }

    public bool IntersectsRay(NetworkGameObject gameObject, Ray ray)
    {
        if(gameObject == null)
            return false;

        float maxDistance = 100;
        PhysicsRayResultCallback rayResult = new PhysicsRayResultCallback(gameObject.Id);
        PhysicsWorld.world.RayTest(ray.origin, 
            ray.origin + (ray.direction * maxDistance),
            rayResult);

        return rayResult.IsHit();
    }
}