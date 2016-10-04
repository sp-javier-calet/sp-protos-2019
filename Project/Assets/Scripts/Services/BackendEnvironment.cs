using SocialPoint.Base;

public enum BackendEnvironment
{
    None,
    Development,
    Production,
    Test,
    Docker}
;

public static class BackendEnvironmentExtensions
{
    const string DevelopmentUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string TestUrl = "http://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string ProductionUrl = "https://int-sp-bootstrap-000a.vpc01.use1.laicosp.net/api/v3";
    const string DockerUrl = "http://localhost:4630/api/v3";

    public static string GetUrl(this BackendEnvironment env)
    {
        var environmentUrl = EnvironmentSettings.Instance.EnvironmentUrl;
        if(!string.IsNullOrEmpty(environmentUrl))
        {
            return environmentUrl;
        }
       
        switch(env)
        {
        case BackendEnvironment.None:
            return null;
        case BackendEnvironment.Development:
            return DevelopmentUrl;
        case BackendEnvironment.Production:
            return ProductionUrl;
        case BackendEnvironment.Test:
            return TestUrl;
        case BackendEnvironment.Docker:
            return DockerUrl;
        }
        return null;
    }
}
