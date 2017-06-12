using NUnit.Framework;
using System.IO;
using System;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Physics;
using Jitter.LinearMath;

namespace SocialPoint.Multiplayer
{
    /*
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class ModelSerializationTests
    {
        [Test]
        public void CompatibilityTest()
        {
            var model1 = new NetworkModel();
            var model2 = new NetworkModel();

            model1.SetData(new TestModelValue0(0));
            model2.SetData(new TestModelValue0(1));

            Assert.IsTrue(AreCompatible(model1, model2));
        }

        [Test]
        public void IncompatibilityTest()
        {
            var model1 = new NetworkModel();
            var model2 = new NetworkModel();

            model1.SetData(new TestModelValue0(0));
            model2.SetData(new TestModelValue1(0));

            Assert.IsTrue(!AreCompatible(model1, model2));
        }

        [Test]
        public void NewModelParseTest()
        {
            var modelSerializer = new NetworkModelSerializer();
            var modelParser = new NetworkModelParser();

            var testSerializer = new TestValueSerializer<TestModelValue0>();
            var testParser = new TestValueParser<TestModelValue0>();

            modelSerializer.RegisterSerializer(TestModelType.Type0, testSerializer);
            modelParser.RegisterParser(TestModelType.Type0, testParser);

            var model1 = new NetworkModel();
            model1.SetData(new TestModelValue0(100));
            var buffer = GetBytesFromModel(modelSerializer, model1);
            var model2 = GetModelFromBytes(modelParser, buffer);

            Assert.IsTrue(AreEqual(modelSerializer, model1, model2));
        }

        [Test]
        public void NewModelReferenceTest()
        {
            var modelSerializer = new NetworkModelSerializer();
            var modelParser = new NetworkModelParser();

            var testSerializer = new TestValueSerializer<TestModelValue0>();
            var testParser = new TestValueParser<TestModelValue0>();

            modelSerializer.RegisterSerializer(TestModelType.Type0, testSerializer);
            modelParser.RegisterParser(TestModelType.Type0, testParser);

            var model1 = new NetworkModel();
            var data1 = new TestModelValue0(100);
            model1.SetData(data1);
            var buffer = GetBytesFromModel(modelSerializer, model1);
            var model2 = GetModelFromBytes(modelParser, buffer);

            Assert.IsTrue(AreEqual(modelSerializer, model1, model2));

            data1.Value = 200;
            Assert.IsTrue(!AreEqual(modelSerializer, model1, model2));
        }

        [Test]
        public void OldModelParseTest()
        {
            var modelSerializer = new NetworkModelSerializer();
            var modelParser = new NetworkModelParser();

            var testSerializer = new TestValueSerializer<TestModelValue0>();
            var testParser = new TestValueParser<TestModelValue0>();

            modelSerializer.RegisterSerializer(TestModelType.Type0, testSerializer);
            modelParser.RegisterParser(TestModelType.Type0, testParser);

            var model1 = new NetworkModel();
            model1.SetData(new TestModelValue0(100));
            var oldModel1 = new NetworkModel();
            oldModel1.SetData(new TestModelValue0(0));

            var model2 = new NetworkModel();
            model2.SetData(new TestModelValue0(0));

            var writeStream = new System.IO.MemoryStream();
            var binWriter = new SystemBinaryWriter(writeStream);
            modelSerializer.Serialize(model1, oldModel1, binWriter);
            var buffer = writeStream.ToArray();

            var readStream = new System.IO.MemoryStream(buffer);
            var binReader = new SystemBinaryReader(readStream);
            modelParser.Parse(model2, binReader);

            Assert.IsTrue(AreEqual(modelSerializer, model1, model2));
        }

        [Test]
        public void OldModelParseNoDiffTest()
        {
            var modelSerializer = new NetworkModelSerializer();
            var modelParser = new NetworkModelParser();

            var testSerializer = new TestValueSerializer<TestModelValue0>();
            var testParser = new TestValueParser<TestModelValue0>();

            modelSerializer.RegisterSerializer(TestModelType.Type0, testSerializer);
            modelParser.RegisterParser(TestModelType.Type0, testParser);

            var model1 = new NetworkModel();
            model1.SetData(new TestModelValue0(100));
            var oldModel1 = new NetworkModel();
            oldModel1.SetData(new TestModelValue0(100));

            var model2 = new NetworkModel();
            model2.SetData(new TestModelValue0(0));
            var oldModel2 = new NetworkModel();
            oldModel2.SetData(new TestModelValue0(0));

            var writeStream = new System.IO.MemoryStream();
            var binWriter = new SystemBinaryWriter(writeStream);
            modelSerializer.Serialize(model1, oldModel1, binWriter);
            var buffer = writeStream.ToArray();

            var readStream = new System.IO.MemoryStream(buffer);
            var binReader = new SystemBinaryReader(readStream);
            modelParser.Parse(model2, binReader);

            Assert.IsTrue(!AreEqual(modelSerializer, model1, model2));
            Assert.IsTrue(AreEqual(modelSerializer, oldModel1, model1));
            Assert.IsTrue(AreEqual(modelSerializer, oldModel2, model2));
        }

        [Test]
        public void OldModelReferenceTest()
        {
            var modelSerializer = new NetworkModelSerializer();
            var modelParser = new NetworkModelParser();

            var testSerializer = new TestValueSerializer<TestModelValue0>();
            var testParser = new TestValueParser<TestModelValue0>();

            modelSerializer.RegisterSerializer(TestModelType.Type0, testSerializer);
            modelParser.RegisterParser(TestModelType.Type0, testParser);

            var model1 = new NetworkModel();
            var data1 = new TestModelValue0(100);
            model1.SetData(data1);
            var oldModel1 = new NetworkModel();
            oldModel1.SetData(new TestModelValue0(0));

            var model2 = new NetworkModel();
            model2.SetData(new TestModelValue0(0));

            var writeStream = new System.IO.MemoryStream();
            var binWriter = new SystemBinaryWriter(writeStream);
            modelSerializer.Serialize(model1, oldModel1, binWriter);
            var buffer = writeStream.ToArray();

            var readStream = new System.IO.MemoryStream(buffer);
            var binReader = new SystemBinaryReader(readStream);
            modelParser.Parse(model2, binReader);

            Assert.IsTrue(AreEqual(modelSerializer, model1, model2));

            data1.Value = 200;
            Assert.IsTrue(!AreEqual(modelSerializer, model1, model2));
        }

        byte[] GetBytesFromModel(NetworkModelSerializer serializer, NetworkModel model)
        {
            var memStream = new System.IO.MemoryStream();
            var binWriter = new SystemBinaryWriter(memStream);
            serializer.Serialize(model, binWriter);
            return memStream.ToArray();
        }

        NetworkModel GetModelFromBytes(NetworkModelParser parser, byte[] buffer)
        {
            var memStream = new System.IO.MemoryStream(buffer);
            var binReader = new SystemBinaryReader(memStream);
            return parser.Parse(binReader);
        }

        bool AreCompatible(NetworkModel m1, NetworkModel m2)
        {
            NetworkModel.ModelablePairOperation empty = (INetworkModelable myModelData, INetworkModelable oldModelData) => {
            };

            return NetworkModel.OperateOverModels(m1, m2, empty);
        }

        bool AreEqual(NetworkModelSerializer serializer, NetworkModel model1, NetworkModel model2)
        {
            bool equal = true;
            NetworkModel.ModelablePairOperation compare = (INetworkModelable myModelData, INetworkModelable otherModelData) => {
                var bytes1 = GetBytesFromModel(serializer, model1);
                var bytes2 = GetBytesFromModel(serializer, model2);
                equal &= AreEqual(bytes1, bytes2);
            };


            bool compatible = NetworkModel.OperateOverModels(model1, model2, compare);
            equal &= compatible;
            return equal;
        }

        bool AreEqual(byte[] bytes1, byte[] bytes2)
        {
            bool equal = bytes1.Length == bytes2.Length;
            if(equal)
            {
                for(int i = 0; i < bytes1.Length; i++)
                {
                    equal &= bytes1[i] == bytes2[i];
                    if(!equal)
                    {
                        break;
                    }
                }
            }
            return equal;
        }

        static class TestModelType
        {
            public const byte Type0 = 0;
            public const byte Type1 = 1;
        }

        class TestModelValueBase
        {
            public int Value;

            public TestModelValueBase(int val)
            {
                Value = val;
            }
        }

        class TestModelValue0 : TestModelValueBase, INetworkModelable
        {
            public TestModelValue0()
                : this(0)
            {
            }

            public TestModelValue0(int val = 0)
                : base(val)
            {
            }

            public byte ModelType
            {
                get
                {
                    return TestModelType.Type0;
                }
            }

            public object Clone()
            {
                return new TestModelValue0(Value);
            }

            public void Copy(object other)
            {
                if(other is TestModelValue0)
                {
                    var otherK = (TestModelValue0)other;
                    Value = otherK.Value;
                }
            }
        }

        class TestModelValue1 : TestModelValueBase, INetworkModelable
        {
            public TestModelValue1()
                : this(0)
            {
            }

            public TestModelValue1(int val = 0)
                : base(val)
            {
            }

            public byte ModelType
            {
                get
                {
                    return TestModelType.Type1;
                }
            }

            public object Clone()
            {
                return new TestModelValue0(Value);
            }

            public void Copy(object other)
            {
                if(other is TestModelValue0)
                {
                    var otherK = (TestModelValue0)other;
                    Value = otherK.Value;
                }
            }
        }

        class TestValueSerializer<T> : IDiffWriteSerializer<T> where T : TestModelValueBase
        {
            public void Compare(T newObj, T oldObj, Bitset dirty)
            {
                dirty.Set(newObj.Value != oldObj.Value);
            }

            public void Serialize(T newObj, IWriter writer)
            {
                writer.Write(newObj.Value);
            }

            public void Serialize(T newObj, T oldObj, IWriter writer, Bitset dirty)
            {
                if(Bitset.NullOrGet(dirty))
                {
                    writer.Write(newObj.Value);
                }
            }
        }

        class TestValueParser<T> : IDiffReadParser<T> where T : TestModelValueBase, new()
        {
            public int GetDirtyBitsSize(T obj)
            {
                return 1;
            }

            public T Parse(IReader reader)
            {
                var obj = new T();
                obj.Value = reader.ReadInt32();
                return obj;
            }

            public T Parse(T oldObj, IReader reader, Bitset dirty)
            {
                if(Bitset.NullOrGet(dirty))
                {
                    oldObj.Value = reader.ReadInt32();
                }
                return oldObj;
            }
        }
    }
    */
}
