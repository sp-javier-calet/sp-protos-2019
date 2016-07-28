using SocialPoint.ScriptEvents;
using System.Collections;
using SocialPoint.Attributes;

public class OrConditionTypeModel : ConditionContainerTypeModel
{
    public OrConditionTypeModel(string goalConditionName, int repetitions, IModelCondition[] conditions) : base(goalConditionName, repetitions, conditions)
    {
    }

    public override bool ValidateEvent(PlayerModel player, Attr eventData)
    {
        int conditionsCount = Conditions.Length;

        for(int index = 0; index < conditionsCount; ++index)
        {
            if(Conditions[index].ValidateEvent(player, eventData))
            {
                return true;
            }
        }

        return false;
    }

    public override bool ValidateModel(PlayerModel player)
    {
        int conditionsCount = Conditions.Length;

        for(int index = 0; index < conditionsCount; ++index)
        {
            if(Conditions[index].ValidateModel(player))
            {
                return true;
            }
        }

        return false;
    }
}
