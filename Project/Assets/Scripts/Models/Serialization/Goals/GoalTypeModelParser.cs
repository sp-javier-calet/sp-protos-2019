using System.Collections;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;
using System;

public class GoalTypeModelParser : IParser<GoalTypeModel>
{
    const string AttrKeyGoalId = "id";
    const string AttrKeyConditions = "conditions";
    const string AttrKeyTutorial = "tutorial";
    const string AttrKeyReward = "reward";

    IParser<IModelCondition> _conditionsParser;
    IParser<IReward> _rewardParser;

    public GoalTypeModelParser(IParser<IModelCondition> conditionsParser, IParser<IReward> rewardParser)
    {
        _conditionsParser = conditionsParser;
        _rewardParser = rewardParser;
    }

    public GoalTypeModel Parse(Attr data)
    {
        var goalId = data.AsDic[AttrKeyGoalId].ToString();

        var conditionsList = data.AsDic[AttrKeyConditions].AsList;
        var conditions = new List<IModelCondition>();

        for(int index = 0; index < conditionsList.Count; ++index)
        {
            conditions.Add(_conditionsParser.Parse(conditionsList[index]));
        }

        return new GoalTypeModel(
            id : goalId,
            conditions : conditions,
            tutorial : data.AsDic[AttrKeyTutorial],
            reward : _rewardParser.Parse(data.AsDic[AttrKeyReward])
        );
    }
}
