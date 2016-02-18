using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.ServerSync
{
    public class Command
    {
        public string Id;
        public string Name;
        public long Timestamp;
        public bool Unique;
        public bool Atomic;
        public Attr Arguments;
        static readonly string AttrKeyName = "cmd";
        static readonly string AttrKeyId = "cid";
        static readonly string AttrKeyTimestamp = "ts";
        static readonly string AttrKeyArguments = "args";
        static readonly string AttrKeyUnique = "unique";
        static readonly string AttrKeyAtomic = "atomic";

        public Command(string name, Attr args = null, bool unique = false, bool atomic = true)
        {
            Id = RandomUtils.GetUuid();
            Name = name;
            Arguments = args;
            Unique = unique;
            Atomic = atomic;
            Timestamp = TimeUtils.Timestamp;
        }

        public Command(Attr data)
        {
            FromAttr(data);
        }

        override public string ToString()
        {
            return ToAttr().ToString();
        }

        public void FromAttr(Attr data)
        {
            var datadic = data.AssertDic;
            Id = datadic.GetValue(AttrKeyId).ToString();
            Name = datadic.GetValue(AttrKeyName).ToString();
            Timestamp = datadic.GetValue(AttrKeyTimestamp).ToLong();
            Unique = datadic.GetValue(AttrKeyUnique).ToBool();
            Atomic = datadic.GetValue(AttrKeyAtomic).ToBool();
            Arguments = datadic.Get(AttrKeyArguments);
        }

        public Attr ToAttr()
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyId, Id);
            data.SetValue(AttrKeyName, Name);
            data.SetValue(AttrKeyUnique, Unique);
            data.SetValue(AttrKeyAtomic, Atomic);
            data.SetValue(AttrKeyTimestamp, Timestamp);
            if(Arguments != null)
            {
                data.Set(AttrKeyArguments, Arguments);
            }
            return data;
        }

        public Attr ToRequestAttr()
        {
            var data = new AttrDic();
            data.SetValue(AttrKeyId, Id);
            data.SetValue(AttrKeyName, Name);
            data.SetValue(AttrKeyTimestamp, Timestamp);
            if(Arguments != null)
            {
                data.Set(AttrKeyArguments, Arguments);
            }
            return data;
        }

        public Error Validate(Attr response)
        {
            return null;
        }
    }

}
