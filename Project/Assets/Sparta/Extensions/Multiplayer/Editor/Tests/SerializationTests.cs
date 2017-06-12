//using NUnit.Framework;
//using System.IO;
//using System;
//using SocialPoint.IO;
//using SocialPoint.Network;
//using SocialPoint.Physics;
//using SocialPoint.Utils;
//using Jitter.LinearMath;
//
//namespace SocialPoint.Multiplayer
//{
//    [TestFixture]
//    [Category("SocialPoint.Multiplayer")]
//    class SerializationTests
//    {
//
//        [Test]
//        public void Transform()
//        {
//            SerializationTestUtils<Transform>.CompleteAndDifference(
//                new Transform(
//                    new JVector(1.0f, 2.3f, 4.2f),
//                    new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
//                    new JVector(2.0f, 1.0f, 2.0f)
//                ),
//                new Transform(
//                    new JVector(1.0f, 3.3f, 4.2f),
//                    new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
//                    new JVector(1.0f, 0.0f, 2.0f)
//                ),
//                new TransformSerializer(),
//                new TransformParser());
//        }
//
//
//        [Test]
//        public void NetworkGameObject()
//        {
//            SerializationTestUtils<NetworkGameObject>.CompleteAndDifference(
//                new NetworkGameObject(1, true, new Transform(
//                    new JVector(1.0f, 2.3f, 4.2f),
//                    new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
//                    new JVector(2.0f, 1.0f, 2.0f)
//                )
//                ),
//                new NetworkGameObject(1, true, new Transform(
//                    new JVector(1.0f, 3.3f, 4.2f),
//                    new JQuaternion(1.0f, 3.3f, 4.2f, 6.0f),
//                    new JVector(1.0f, 0.0f, 2.0f)
//                )
//                ),
//                new NetworkGameObjectSerializer(),
//                new NetworkGameObjectParser());
//        }
//
//        [Test]
//        public void NetworkGameScene()
//        {
//            var scene = new NetworkScene();
//
//            scene.AddObject(new NetworkGameObject(1, true, new Transform(
//                new JVector(1.0f, 2.3f, 4.2f),
//                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(2.0f, 1.0f, 2.0f)
//            )));
//
//            var scene2 = new NetworkScene();
//
//            scene2.AddObject(new NetworkGameObject(1, true, new Transform(
//                new JVector(1.0f, 2.3f, 4.6f),
//                new JQuaternion(3.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(2.0f, 1.0f, 4.0f)
//            )));
//
//            scene2.AddObject(new NetworkGameObject(2, true, new Transform(
//                new JVector(2.0f, 2.3f, 4.2f),
//                new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(3.0f, 1.0f, 2.1f)
//            )));
//
//            SerializationTestUtils<NetworkScene>.CompleteAndDifference(
//                scene2, scene,
//                new NetworkSceneSerializer(),
//                new NetworkSceneParser());
//        }
//
//        [Test]
//        public void NetworkGameSceneRemoveDiff()
//        {
//            var scene = new NetworkScene();
//
//            scene.AddObject(new NetworkGameObject(1, true, new Transform(
//                new JVector(1.0f, 2.3f, 4.2f),
//                new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(2.0f, 1.0f, 2.0f)
//            )));
//
//            scene.AddObject(new NetworkGameObject(2, true, new Transform(
//                new JVector(2.0f, 2.3f, 4.2f),
//                new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(3.0f, 1.0f, 2.1f)
//            )));
//
//            var scene2 = new NetworkScene();
//
//            scene2.AddObject(new NetworkGameObject(1, true, new Transform(
//                new JVector(1.0f, 2.3f, 4.6f),
//                new JQuaternion(3.0f, 2.3f, 4.2f, 5.0f),
//                new JVector(2.0f, 1.0f, 4.0f)
//            )));
//
//            SerializationTestUtils<NetworkScene>.Difference(
//                scene2, scene,
//                new NetworkSceneSerializer(),
//                new NetworkSceneParser());
//        }
//
//        [Test]
//        public void NetworkGameSceneCopyData()
//        {
//            var scene1 = new NetworkScene();
//            var scene2 = new NetworkScene();
//
//            var goRef1 = new NetworkGameObject(1, true, new Transform(
//                             new JVector(1.0f, 2.3f, 4.2f),
//                             new JQuaternion(1.0f, 2.3f, 4.2f, 5.0f),
//                             new JVector(2.0f, 1.0f, 2.0f)
//                         ));
//
//            var goRef2 = new NetworkGameObject(2, true, new Transform(
//                             new JVector(2.0f, 2.3f, 4.2f),
//                             new JQuaternion(5.0f, 2.3f, 4.2f, 5.0f),
//                             new JVector(3.0f, 1.0f, 2.1f)
//                         ));
//
//            var goRef3 = new NetworkGameObject(3, true, new Transform(
//                             new JVector(3.0f, 2.3f, 4.2f),
//                             new JQuaternion(9.0f, 2.3f, 4.2f, 5.0f),
//                             new JVector(4.0f, 1.0f, 2.1f)
//                         ));
//
//            scene1.AddObject(goRef1);
//            scene1.AddObject(goRef2);
//
//            scene2.AddObject(goRef1);
//            scene2.AddObject(goRef3);
//
//            scene2.Copy(scene1);
//
//            var goRef1b = scene2.FindObject(1);
//            goRef1b.Transform.Position = JVector.Zero;
//            Assert.That(goRef1b != null && goRef1.Equals(goRef1b));
//
//            var goRef2b = scene2.FindObject(2);
//            Assert.That(goRef2b != null);
//
//            var goRef3b = scene2.FindObject(3);
//            Assert.That(goRef3b == null);
//
//            SerializationTestUtils<NetworkScene>.Difference(
//                scene2, scene1,
//                new NetworkSceneSerializer(),
//                new NetworkSceneParser());
//        }
//
//        [Test]
//        public void Behaviour()
//        {
//            var serializer = new NetworkBehaviourContainerSerializer<INetworkBehaviour>();
//            serializer.Register(1, new TestObjectSerializer());
//
//            var parser = new NetworkBehaviourContainerParser<INetworkBehaviour>();
//            parser.Register(1, new TestObjectParser());
//
//            var newObj = new NetworkBehaviourContainer<INetworkBehaviour>();
//            newObj.Add(new TestObjectSyncBehaviour(5, "test"));
//
//            var oldObj = new NetworkBehaviourContainer<INetworkBehaviour>();
//            oldObj.Add(new TestObjectSyncBehaviour(2, "test"));
//
//            SerializationTestUtils<NetworkBehaviourContainer<INetworkBehaviour>>
//                .CompleteAndDifference(newObj, oldObj, serializer, parser);
//        }
//
//        [Test]
//        public void GameObjectBehaviour()
//        {
//            var newObj = new NetworkGameObject<INetworkBehaviour>(1);
//            newObj.AddBehaviour(new TestObjectSyncBehaviour(5, "test2"));
//
//            var oldObj = new NetworkGameObject<INetworkBehaviour>(1);
//            oldObj.AddBehaviour(new TestObjectSyncBehaviour(5, "test"));
//
//            var serializer = new NetworkGameObjectSerializer<INetworkBehaviour>();
//            serializer.RegisterBehaviour(1, new TestObjectSerializer());
//
//            var parser = new NetworkGameObjectParser<INetworkBehaviour>();
//            parser.RegisterBehaviour(1, new TestObjectParser());
//
//            SerializationTestUtils<NetworkGameObject>
//                .CompleteAndDifference(newObj, oldObj, serializer, parser);
//        }
//
//        [Test]
//        public void SceneBehaviour()
//        {
//            var newScene = new NetworkScene<INetworkSceneBehaviour>();
//            newScene.AddBehaviour(new TestSceneSyncBehaviour(5, "test2"));
//            var newObj = new NetworkGameObject<INetworkBehaviour>(1);
//            newObj.AddBehaviour(new TestObjectSyncBehaviour(5, "test2"));
//            newScene.AddObject(newObj);
//
//            var oldScene = new NetworkScene<INetworkSceneBehaviour>();
//            oldScene.AddBehaviour(new TestSceneSyncBehaviour(5, "test2"));
//            var oldObj = new NetworkGameObject<INetworkBehaviour>(1);
//            oldObj.AddBehaviour(new TestObjectSyncBehaviour(6, "test"));
//            oldScene.AddObject(newObj);
//
//            var objectSerializer = new NetworkGameObjectSerializer<INetworkBehaviour>();
//            objectSerializer.RegisterBehaviour(1, new TestObjectSerializer());
//
//            var sceneSerializer = new NetworkSceneSerializer<INetworkSceneBehaviour>(
//                objectSerializer);
//            sceneSerializer.RegisterSceneBehaviour(1, new TestSceneSerializer());
//
//            var objectParser = new NetworkGameObjectParser<INetworkBehaviour>();
//            objectParser.RegisterBehaviour(1, new TestObjectParser());
//
//            var sceneParser = new NetworkSceneParser<INetworkSceneBehaviour>(
//                objectParser);
//            sceneParser.RegisterSceneBehaviour(1, new TestSceneParser());
//
//            SerializationTestUtils<NetworkScene<INetworkSceneBehaviour>>
//                .CompleteAndDifference(newScene, oldScene, sceneSerializer, sceneParser);
//        }
//    }
//
//    public class TestObjectSyncBehaviour : NetworkBehaviour
//    {
//        public int Data1;
//        public string Data2;
//
//        public TestObjectSyncBehaviour(int data1 = 0, string data2 = null)
//        {
//            Data1 = data1;
//            Data2 = data2;
//        }
//
//        public override object Clone()
//        {
//            return new TestObjectSyncBehaviour(Data1, Data2);
//        }
//
//        public override int GetHashCode()
//        {
//            var hash = Data1.GetHashCode();
//            hash = CryptographyUtils.HashCombine(hash, Data2.GetHashCode());
//            return hash;
//        }
//
//        public override bool Equals(object obj)
//        {
//            var behaviour = obj as TestObjectSyncBehaviour;
//            if(behaviour == null)
//            {
//                return false;
//            }
//            return behaviour.Data1 == Data1 && behaviour.Data2 == Data2;
//        }
//    }
//
//    public class TestObjectSerializer : IDiffWriteSerializer<TestObjectSyncBehaviour>
//    {
//        public void Compare(TestObjectSyncBehaviour newObj, TestObjectSyncBehaviour oldObj, Bitset dirty)
//        {
//            dirty.Set(newObj.Data1 != oldObj.Data1);
//            dirty.Set(newObj.Data2 != oldObj.Data2);
//        }
//
//        public void Serialize(TestObjectSyncBehaviour newObj, TestObjectSyncBehaviour oldObj, IWriter writer, Bitset dirty)
//        {
//            if(Bitset.NullOrGet(dirty))
//            {
//                writer.Write(newObj.Data1);
//            }
//            if(Bitset.NullOrGet(dirty))
//            {
//                writer.Write(newObj.Data2);
//            }
//        }
//
//        public void Serialize(TestObjectSyncBehaviour newObj, IWriter writer)
//        {
//            writer.Write(newObj.Data1);
//            writer.Write(newObj.Data2);
//        }
//    }
//
//    public class TestObjectParser : IDiffReadParser<TestObjectSyncBehaviour>
//    {
//        public TestObjectSyncBehaviour Parse(TestObjectSyncBehaviour oldObj, IReader reader, Bitset dirty)
//        {
//            if(Bitset.NullOrGet(dirty))
//            {
//                oldObj.Data1 = reader.ReadInt32();
//            }
//            if(Bitset.NullOrGet(dirty))
//            {
//                oldObj.Data2 = reader.ReadString();
//            }
//            return oldObj;
//        }
//
//        public int GetDirtyBitsSize(TestObjectSyncBehaviour obj)
//        {
//            return 2;
//        }
//
//        public TestObjectSyncBehaviour Parse(IReader reader)
//        {
//            var behaviour = new TestObjectSyncBehaviour();
//            behaviour.Data1 = reader.ReadInt32();
//            behaviour.Data2 = reader.ReadString();
//            return behaviour;
//        }
//    }
//
//    public class TestSceneSyncBehaviour : INetworkSceneBehaviour
//    {
//        public int Data1;
//        public string Data2;
//
//        public TestSceneSyncBehaviour(int data1 = 0, string data2 = null)
//        {
//            Data1 = data1;
//            Data2 = data2;
//        }
//
//        NetworkScene INetworkSceneBehaviour.Scene
//        {
//            set
//            {
//            }
//        }
//
//        void INetworkSceneBehaviour.OnStart()
//        {
//        }
//
//        void INetworkSceneBehaviour.OnDestroy()
//        {
//        }
//
//        void INetworkSceneBehaviour.Update(float dt)
//        {
//        }
//
//        void INetworkSceneBehaviour.OnInstantiateObject(NetworkGameObject go)
//        {
//        }
//
//        void INetworkSceneBehaviour.OnDestroyObject(int id)
//        {
//        }
//
//        public override int GetHashCode()
//        {
//            var hash = Data1.GetHashCode();
//            hash = CryptographyUtils.HashCombine(hash, Data2.GetHashCode());
//            return hash;
//        }
//
//        public override bool Equals(object obj)
//        {
//            var behaviour = obj as TestSceneSyncBehaviour;
//            if(behaviour == null)
//            {
//                return false;
//            }
//            return behaviour.Data1 == Data1 && behaviour.Data2 == Data2;
//        }
//    }
//
//    public class TestSceneSerializer : IDiffWriteSerializer<TestSceneSyncBehaviour>
//    {
//        public void Compare(TestSceneSyncBehaviour newObj, TestSceneSyncBehaviour oldObj, Bitset dirty)
//        {
//            dirty.Set(newObj.Data1 != oldObj.Data1);
//            dirty.Set(newObj.Data2 != oldObj.Data2);
//        }
//
//        public void Serialize(TestSceneSyncBehaviour newObj, TestSceneSyncBehaviour oldObj, IWriter writer, Bitset dirty)
//        {
//            if(Bitset.NullOrGet(dirty))
//            {
//                writer.Write(newObj.Data1);
//            }
//            if(Bitset.NullOrGet(dirty))
//            {
//                writer.Write(newObj.Data2);
//            }
//        }
//
//        public void Serialize(TestSceneSyncBehaviour newObj, IWriter writer)
//        {
//            writer.Write(newObj.Data1);
//            writer.Write(newObj.Data2);
//        }
//    }
//
//    public class TestSceneParser : IDiffReadParser<TestSceneSyncBehaviour>
//    {
//        public TestSceneSyncBehaviour Parse(TestSceneSyncBehaviour oldObj, IReader reader, Bitset dirty)
//        {
//            if(Bitset.NullOrGet(dirty))
//            {
//                oldObj.Data1 = reader.ReadInt32();
//            }
//            if(Bitset.NullOrGet(dirty))
//            {
//                oldObj.Data2 = reader.ReadString();
//            }
//            return oldObj;
//        }
//
//        public int GetDirtyBitsSize(TestSceneSyncBehaviour obj)
//        {
//            return 2;
//        }
//
//        public TestSceneSyncBehaviour Parse(IReader reader)
//        {
//            var behaviour = new TestSceneSyncBehaviour();
//            behaviour.Data1 = reader.ReadInt32();
//            behaviour.Data2 = reader.ReadString();
//            return behaviour;
//        }
//    }
//}
