//-----------------------------------------------------------------------
// GameModel.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using SocialPoint.GameData;
using UnityEngine;

namespace GameLoading
{
    [CreateAssetMenu(menuName = "Sparta/Game/Model")]
    public class GameModel : BaseGameModel
    {
        public override bool IsBetterThan(BaseGameModel other)
        {
            // No model info to compare progression, so keep first
            return true;
        }
    }
}
