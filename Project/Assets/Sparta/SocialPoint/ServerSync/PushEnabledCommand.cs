using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class PushEnabledCommand : Command
    {
        static readonly string TypeName = "push_enabled";
        static readonly string TokenKey = "token";
        static readonly string TokenUserAllowsNotifcation = "user_allows_notification";

        public PushEnabledCommand(string token, bool userAllowsNotification) : base(TypeName)
        {
            Atomic = true;
            var args = new AttrDic();
            args.SetValue(TokenKey, token);
            args.SetValue(TokenUserAllowsNotifcation, userAllowsNotification);
            Arguments = args;
        }
    }
}