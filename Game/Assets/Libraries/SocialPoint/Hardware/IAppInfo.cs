namespace SocialPoint.Hardware
{
    public interface IAppInfo
    {
        string SeedId { get; }

        string Id { get; }

        string Version { get; }

        string ShortVersion { get; }

        string Language { get; }

        string Country { get; }
    }
}