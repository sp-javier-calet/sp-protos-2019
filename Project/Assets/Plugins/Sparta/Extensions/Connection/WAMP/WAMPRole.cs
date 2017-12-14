using SocialPoint.Attributes;

namespace SocialPoint.WAMP
{
    public abstract class WAMPRole
    {
        protected readonly WAMPConnection _connection;

        protected WAMPRole(WAMPConnection connection)
        {
            _connection = connection;
        }

        internal abstract void AddRoleDetails(AttrDic detailsDic);

        internal abstract void ResetToInitialState();
    }
}
