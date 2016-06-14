using SocialPoint.Base;

/// <summary>
/// Represents a reward that will be given to the player
/// should  be inmutable
/// </summary>
public interface IReward
{
    ModelError Obtain(PlayerModel playerModel);
}