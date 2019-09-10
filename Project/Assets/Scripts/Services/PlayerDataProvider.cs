//-----------------------------------------------------------------------
// PlayerDataProvider.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Social;

/// <summary>
/// Game must integrate data to implement an unique provider of player data
/// </summary>
class PlayerDataProvider : IPlayerData
{
    [RequiredDependency]
    IUserService _userService;

    #region IPlayerData implementation

    public string Id => _userService.UserId.ToString();

    public string Name => "Player Name";

    public int Level => 0; // TODO IVAN

    #endregion
}

