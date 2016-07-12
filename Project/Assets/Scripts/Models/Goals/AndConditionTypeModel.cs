using SocialPoint.ScriptEvents;
using System.Collections.Generic;
using SocialPoint.Attributes;

public class AndConditionTypeModel : ConditionContainerTypeModel
{
    HashSet<IModelCondition> _validatedConditions = new HashSet<IModelCondition>();

    public AndConditionTypeModel(string goalConditionName, int repetitions, IModelCondition[] conditions) : base(goalConditionName, repetitions, conditions)
    {
    }

    public override bool ValidateEvent(PlayerModel player, Attr eventData)
    {
        return Validate(player, eventData, true);
    }

    public override bool ValidateModel(PlayerModel player)
    {
        return Validate(player, null, false);
    }

    bool Validate(PlayerModel player, Attr eventData, bool isEvent)
    {
        int validatedConditions = 0;
        int conditionsCount = Conditions.Length;

        for(int index = 0; index < conditionsCount; ++index)
        {
            var condition = Conditions[index];

            if(_validatedConditions.Contains(condition))
            {
                validatedConditions++;
                continue;
            }

            bool validated = isEvent ? condition.ValidateEvent(player, eventData) : condition.ValidateModel(player);

            if(validated)
            {
                validatedConditions++;
                _validatedConditions.Add(condition);
            }
        }

        return validatedConditions == conditionsCount;
    }
}
