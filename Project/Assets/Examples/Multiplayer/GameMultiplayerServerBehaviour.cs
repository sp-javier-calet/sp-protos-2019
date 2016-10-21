using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.IO;
using SocialPoint.Multiplayer;
using SocialPoint.Network;
using SocialPoint.Pathfinding;
using System;
using System.IO;
using System.Collections.Generic;
using Jitter;
using Jitter.LinearMath;
using Jitter.Dynamics;
using Jitter.Collision;
using SharpNav;
using SharpNav.Geometry;
using SharpNav.Pathfinding;

public class GameMultiplayerServerBehaviour : INetworkServerSceneReceiver, IDisposable
{
    INetworkServer _server;
    NetworkServerSceneController _controller;

    Dictionary<int,int> _updateTimes;
    float _moveInterval = 1.0f;
    float _timeSinceLastMove = 0.0f;
    int _maxUpdateTimes = 3;
    JVector _movement;
    NetworkGameObject _playerCube;

    PhysicsWorld _physicsWorld;
    IPhysicsDebugger _physicsDebugger;

    int _maxPlayers = 4;
    int _currentPlayers = 0;

    TiledNavMesh _navMesh;
    Pathfinder _pathfinder;

    public int MaxPlayers
    {
        get
        {
            return _maxPlayers;
        }
        private set
        {
            _maxPlayers = value;
        }
    }

    public bool Full
    {
        get
        {
            return (_currentPlayers >= _maxPlayers);
        }
    }

    public string NavMeshPath
    {
        get; set;
    }

    public GameMultiplayerServerBehaviour(INetworkServer server, NetworkServerSceneController ctrl, IPhysicsDebugger physicsDebugger)
    {
        _server = server;
        _controller = ctrl;
        _controller.RegisterReceiver(this);
        _controller.RegisterActionDelegate<MovementAction>(MovementAction.Apply);
        _updateTimes = new Dictionary<int,int>();
        _movement = new JVector(2.0f, 0.0f, 2.0f);

        _physicsDebugger = physicsDebugger;
        AddPhysicsWorld();
    }

    public void Dispose()
    {
        _controller.RegisterReceiver(null);
    }

    public bool LoadNavMesh(string path, out string message)
    {
        message = string.Empty;
        try
        {
            var stream = new FileStream(path, FileMode.Open);
            _navMesh = NavMeshParser.Instance.Parse(new SystemBinaryReader(stream));
            _pathfinder = new Pathfinder(_navMesh);
            return true;
        }
        catch (Exception e)
        {
            message = e.Message;
            return false;
        }
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
                if(itr.Current == _playerCube)
                {
                    //Using first cube as MovementAction target
                    continue;
                }

                var id = itr.Current.Id;
                var p = itr.Current.Transform.Position;

                var deltaX = RandomUtils.Range(-_movement.X, _movement.X);
                var deltaZ = RandomUtils.Range(-_movement.Z, _movement.Z);

                p += new JVector(deltaX, 0.0f, deltaZ);

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
            if(!ClosestIntersectsRay(_playerCube, ac.Ray))
            {
                NetworkGameObject currentCube = _controller.Instantiate("Cube", new Transform(
                                                    ac.Position, JQuaternion.Identity, JVector.One));

                if(_playerCube == null)
                {
                    _playerCube = currentCube;
                }

                AddCollision(currentCube);

                PathfindToTarget(ac.Position);
            }
            else
            {
                Log.i("Raycast over player!");
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
        _currentPlayers++;
    }

    void INetworkServerSceneBehaviour.OnClientDisconnected(byte clientId)
    {
        _currentPlayers--;
    }

    void AddPhysicsWorld()
    {
        _physicsWorld = new PhysicsWorld(true);

        _controller.AddBehaviour(_physicsWorld);
    }

    void AddCollision(NetworkGameObject go)
    {
        var boxShape = new PhysicsBoxShape(new JVector(1f));
        var rigidBody = new PhysicsRigidBody(boxShape, PhysicsRigidBody.ControlType.Kinematic, _physicsWorld, _physicsDebugger);
        rigidBody.DoDebugDraw = true;

        if(go.Id == _playerCube.Id)
        {
            rigidBody.AddCollisionHandler(PlayerCollisionHandler);
        }

        _controller.AddBehaviour(go.Id, rigidBody);
    }

    public bool ClosestIntersectsRay(NetworkGameObject gameObject, Ray ray)
    {
        if(gameObject == null)
        {
            return false;
        }

        float maxDistance = 100f;
        PhysicsRaycast.Result rayResultClosest;

        if(PhysicsRaycast.Raycast(ray, maxDistance, _physicsWorld, out rayResultClosest))
        {
            if(rayResultClosest.ObjectHit.NetworkGameObject.Id == gameObject.Id)
            {
                return true;
            }
        }

        return false;
    }

    void PlayerCollisionHandler(RigidBody body1, RigidBody body2, 
                                JVector point1, JVector point2, JVector normal, float penetration)
    {
        Log.i("Player Collision Detected!");
    }

    void PathfindToTarget(JVector target)
    {
        if (_playerCube != null && _pathfinder != null)
        {
            Vector3 startPoint = _playerCube.Transform.Position.ToPathfinding();
            Vector3 endPoint = target.ToPathfinding();
            var extents = Vector3.One;
            StraightPath straightPath;
            if(_pathfinder.TryGetPath(startPoint, endPoint, extents, out straightPath))
            {
                Log.i("Path Found!");
            }
        }
    }
}