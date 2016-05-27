
using SocialPoint.Base;
using System;

public abstract class CostError
{
}

public class CostException : Exception
{
    public CostError CostError { get; private set; }

    public CostException(CostError costError) : base(costError.ToString())
    {
        CostError = costError;
    }
}

public interface ICost
{
    void Validate(PlayerModel playerModel, Action<CostError> finished);

    void Spend(PlayerModel playerModel);
}

public static class CostExtensions
{
    public static void ValidateAndSpend(this ICost cost, PlayerModel playerModel, Action<CostError> finished)
    {
        cost.Validate(playerModel, (CostError error) => {
            if(error == null)
            {
                cost.Spend(playerModel);
            }
            if(finished != null)
            {
                finished(error);
            }
        });
    }
}