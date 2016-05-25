
using SocialPoint.Base;
using System;

public interface ICost
{
    void Validate(PlayerModel playerModel, Action<Error> finished);

    void Spend(PlayerModel playerModel);
}

public static class CostExtensions
{
    public static void ValidateAndSpend(this ICost cost, PlayerModel playerModel, Action<Error> finished)
    {
        cost.Validate(playerModel, (Error error) => {
            if(Error.IsNullOrEmpty(error))
            {
                cost.Spend(playerModel);
            }
            finished(error);
        });
    }
}