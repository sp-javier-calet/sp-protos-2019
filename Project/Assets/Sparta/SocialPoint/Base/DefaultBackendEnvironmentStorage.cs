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
                var value = DebugUtils.IsDebugBuild ? _default : _production;
                #if ADMIN_PANEL
                value = _default;
                #endif
                return value;
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