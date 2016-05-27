
using SocialPoint.Base;
using System;


public class CostException : Exception
{
    public ModelError Error { get; private set; }

    public CostException(ModelError error) : base(error.ToString())
    {
        Error = error;
    }
}

public interface ICost
{
    void Validate(PlayerModel playerModel, Action<ModelError> finished);

    void Spend(PlayerModel playerModel);
}

public static class CostExtensions
{
    public static void ValidateAndSpend(this ICost cost, PlayerModel playerModel, Action<ModelError> finished)
    {
        cost.Validate(playerModel, (ModelError error) => {
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