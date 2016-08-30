using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class EventCommand : Command
    {
        static readonly string TypeName = "event";
        static readonly string NameKey = "type";
        static readonly string DataKey = "data";

        public EventCommand(string name, Attr data) : base(TypeName)
        {
            Atomic = false;
            var args = new AttrDic();
            args.SetValue(NameKey, name);
            args.Set(DataKey, data);
            Arguments = args;
        }
    }

}
