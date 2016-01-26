public enum BackendEnvironment
{
    None,
    Develpoment,
    Production,
    Test,
    Docker
};

public static class BackendEnvironmentExtensions
{
    const string DevelopmentUrl = "http://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";
    const string TestUrl = "http://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";
    const string ProductionUrl = "https://pro-tech-bootstrap-000a.pro.tech.laicosp.net/api/v3";
    const string DockerUrl = "http://localhost:4630/api/v3";

    public static string GetUrl(this BackendEnvironment env)
    {
        switch(env)
        {
        case BackendEnvironment.None:
            return null;
        case BackendEnvironment.Develpoment:
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
