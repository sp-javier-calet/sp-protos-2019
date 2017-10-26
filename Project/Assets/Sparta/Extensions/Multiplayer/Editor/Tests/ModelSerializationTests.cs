using NUnit.Framework;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    [TestFixture]
    [Category("SocialPoint.Multiplayer")]
    class ModelSerializationTests
    {
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

        class TestModelValue0 : TestModelValueBase
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
                var testModelValue0 = other as TestModelValue0;
                if(testModelValue0 != null)
                {
                    Value = testModelValue0.Value;
                }
            }
        }

        class TestModelValue1 : TestModelValueBase
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
                var testModelValue0 = other as TestModelValue0;
                if(testModelValue0 != null)
                {
                    Value = testModelValue0.Value;
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
}
