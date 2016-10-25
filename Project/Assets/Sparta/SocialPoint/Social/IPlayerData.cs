namespace SocialPoint.Social
{
    /// <summary>
    /// Model-related Player Data
    /// </summary>
    public interface IPlayerData
    {
        /// <summary>
        /// User Id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Player Name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Player Level
        /// </summary>
        long Level { get; }
    }
}
