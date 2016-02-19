using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public class PushEnabledCommand : Command
    {
        static readonly string TypeName = "push_enabled";
        static readonly string TokenKey = "token";

        public PushEnabledCommand(string token) : base(TypeName)
        {
            Atomic = true;
            var args = new AttrDic();
            args.SetValue(TokenKey, token);
            Arguments = args;
        }
    }
}