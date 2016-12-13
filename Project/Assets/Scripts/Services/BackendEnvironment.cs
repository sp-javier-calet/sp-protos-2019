using SocialPoint.Base;

public enum BackendEnvironment
{
    None,
    JenkinsForced,
    Development,
    Production,
    Test,
    Docker,
    LeagueOfDragons
}

public static class BackendEnvironmentExtensions
{
    const string DevelopmentUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string TestUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string ProductionUrl = "https://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string DockerUrl = "http://localhost:4630/api/v3";
    const string LeagueOfDragonsUrl = "http://int-lod.socialpointgames.com/lod3/web/index.php/api/v3";

    static string JenkinsForcedUrl
    {
        get
        {
            var environmentUrl = EnvironmentSettings.Instance.EnvironmentUrl;
            if(!string.IsNullOrEmpty(environmentUrl))
            {
                return environmentUrl;
            }
            return DebugUtils.IsDebugBuild ? DevelopmentUrl : ProductionUrl;
        }
    }

    public static string GetUrl(this BackendEnvironment env)
    {
        switch(env)
        {
        case BackendEnvironment.None:
            return null;
        case BackendEnvironment.JenkinsForced:
            return JenkinsForcedUrl;
        case BackendEnvironment.Development:
            return DevelopmentUrl;
        case BackendEnvironment.Production:
            return ProductionUrl;
        case BackendEnvironment.Test:
            return TestUrl;
        case BackendEnvironment.Docker:
            return DockerUrl;
        case BackendEnvironment.LeagueOfDragons:
            return LeagueOfDragonsUrl;
        }
        return null;
    }
}
