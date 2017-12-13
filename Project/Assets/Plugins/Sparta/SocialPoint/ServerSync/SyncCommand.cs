using System;
using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class SyncCommand : Command
    {
        static readonly string DefaultKey = "data";
        static readonly string TypeName = "sync";
        static readonly string KeyKey = "key";
        static readonly string ValueKey = "value";

        public SyncCommand(Attr value) :
            this(DefaultKey, value)
        {
        }

        public SyncCommand(byte[] value) :
            this(DefaultKey, value)
        {
        }

        public SyncCommand(string value) :
            this(DefaultKey, value)
        {
        }

        AttrDic Init(string key)
        {
            Unique = true;
            Atomic = true;
            var args = new AttrDic();
            args.SetValue(KeyKey, key);
            Arguments = args;
            return args;
        }

        public SyncCommand(string key, Attr value) : base(TypeName)
        {
            var args = Init(key);
            args.Set(ValueKey, value);
        }

        public SyncCommand(string key, string value) : base(TypeName)
        {
            var args = Init(key);
            args.SetValue(ValueKey, value);
        }

        public SyncCommand(string key, byte[] value) : base(TypeName)
        {
            var args = Init(key);
            args.SetValue(ValueKey, Convert.ToBase64String(value));
        }
    }

}
