namespace SocialPoint.Base
{
    public sealed class DefaultBackendEnvironmentStorage : IBackendEnvironmentStorage
    {
        readonly string _default;
        readonly string _production;

        public DefaultBackendEnvironmentStorage(string productionEnvironment, string defaultEnvironment)
        {
            _default = defaultEnvironment;
            _production = productionEnvironment;
        }

        public string Default
        {
            get
            {
                #if ADMIN_PANEL
                return _default;
                #endif

                return DebugUtils.IsDebugBuild ? _default : _production;
            }
        }

        public string Selected
        {
            set
            {
            }
            get
            {
                return null;
            }
        }
    }
}