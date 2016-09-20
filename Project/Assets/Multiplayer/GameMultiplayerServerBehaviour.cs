using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using System;
using System.Collections.Generic;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;

public class GameMultiplayerServerBehaviour : INetworkServerSceneReceiver, IDisposable
{
    INetworkServer _server;
    NetworkServerSceneController _controller;

    Dictionary<int,int> _updateTimes;
    float _moveInterval = 1.0f;
    float _timeSinceLastMove = 0.0f;
    int _maxUpdateTimes = 3;
    JVector _movement;
    NetworkGameObject playerCube = null;

    PhysicsWorld _physicsWorld;
    PhysicsDebugger _physicsDebugger;

    public GameMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl)
    {
        _server = server;
        _controller = ctrl;
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(MovementAction.Apply);
        _updateTimes = new Dictionary<int,int>();
        _movement = new JVector(2.0f, 0.0f, 2.0f);

        _physicsDebugger = new UnityPhysicsDebugger();
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

                p += new JVector(
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
            if(!ClosestIntersectsRay(playerCube, ac.Ray))
            {
                NetworkGameObject currentCube = _controller.Instantiate("Cube", new Transform(
                                                    ac.Position, JQuaternion.Identity, JVector.One));

                if(playerCube == null)
                {
                    playerCube = currentCube;
                }

                AddCollision(currentCube);
            }
            else
            {
                _physicsDebugger.Log("Raycast over player!");
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
        _physicsWorld = new PhysicsWorld(true);
        _physicsWorld.AddCollisionHandler(CollisionDetectedHandler);

        _controller.AddBehaviour(_physicsWorld);
    }

    void AddCollision(NetworkGameObject go)
    {
        var boxShape = new PhysicsBoxShape(new JVector(1f));
        var rigidBody = new PhysicsRigidBody(boxShape, PhysicsRigidBody.ControlType.Kinematic, _physicsWorld, _physicsDebugger);
        rigidBody.DoDebugDraw = true;
        //var collCallback = new DemoCollisionCallbackListener(rigidBody.CollisionObject, _physicsDebugger);
        //rigidBody.AddOnCollisionCallbackEventHandler(collCallback);

        _controller.AddBehaviour(go.Id, rigidBody);
    }

    public bool ClosestIntersectsRay(NetworkGameObject gameObject, Ray ray)
    {
        if(gameObject == null)
        {
            return false;
        }

        //float maxDistance = 100f;
        /*var rayResultClosest = new PhysicsRaycast.ClosestResult();

        if(PhysicsRaycast.Raycast(ray, maxDistance, _physicsWorld, out rayResultClosest))
        {
            if(rayResultClosest.GameObjectHit.Id == gameObject.Id)
            {
                return true;
            }
        }*/
        //RigidBody resBody;
        //JVector hitNormal;
        //float fraction;

        /*if(_physicsWorld.CollisionWorld.CollisionSystem.Raycast(ray.origin, ray.direction * maxDistance, null, out resBody, out hitNormal, out fraction))
        {
            if(((PhysicsRigidBody)resBody.Tag).NetworkGameObject.Id == gameObject.Id)
            {
                return true;
            }
        }*/

        return false;
    }

    void CollisionDetectedHandler(RigidBody body1, RigidBody body2, 
                                  JVector point1, JVector point2, JVector normal, float penetration)
    {
        UnityEngine.Debug.Log("*** TEST Collision Detected");
    }
}