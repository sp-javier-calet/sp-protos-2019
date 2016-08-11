using System.Collections.Generic;
using SocialPoint.IO;
using System.Collections;

namespace SocialPoint.Multiplayer
{
    public class NetworkGameScene
    {
        public List<NetworkGameObject> Objects = new List<NetworkGameObject>();

        public void Add(NetworkGameObject obj)
        {
            Objects.Add(obj);
        }
    }

    public class NetworkGameSceneSerializer : ISerializer<NetworkGameScene>
    {
        NetworkGameObjectSerializer _go = new NetworkGameObjectSerializer();

        public void Compare(NetworkGameScene newScene, NetworkGameScene oldScene, DirtyBits dirty)
        {            
        }

        public void Serialize(NetworkGameScene newScene, IWriter writer)
        {
            var c = newScene.Objects.Count;
            writer.Write(c);
            for(var i = 0; i < c; i++)
            {
                var go = newScene.Objects[i];
                _go.Serialize(go, writer);
            }
        }

        public void Serialize(NetworkGameScene newScene, NetworkGameScene oldObj, IWriter writer, DirtyBits dirty)
        {
            var c = newScene.Objects.Count;
            writer.Write(c);
            for(var i = 0; i < c; i++)
            {
                var go = newScene.Objects[i];
                writer.Write(go.Id);
                _go.Serialize(go, writer);
            }
        }
    }

    public class NetworkGameSceneParser : IParser<NetworkGameScene>
    {
        NetworkGameObjectParser _go = new NetworkGameObjectParser();

        public NetworkGameScene Parse(IReader reader)
        {
            var obj = new NetworkGameScene();
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var go = _go.Parse(reader);
                obj.Objects.Add(go);
            }
            return obj;
        }

        public NetworkGameScene Parse(NetworkGameScene scene, IReader reader)
        {
            var c = reader.ReadInt32();
            for(var i = 0; i < c; i++)
            {
                var id = reader.ReadInt32();
                var go = scene.Objects.Find(e => e.Id == id);
                if(go == null)
                {
                    go = _go.Parse(reader);
                    scene.Objects.Add(go);
                }
                else
                {
                    _go.Parse(go, reader);
                }
            }

            return scene;
        }
    }
}
