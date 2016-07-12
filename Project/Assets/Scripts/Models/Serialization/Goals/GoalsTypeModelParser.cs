using SocialPoint.Attributes;
using System.Collections.Generic;
using SocialPoint.ScriptEvents;

public class GoalsTypeModelParser : IParser<GoalsTypeModel>
{
    IParser<IModelCondition> _conditionsParser;
    IParser<IReward> _rewardParser;

    public GoalsTypeModelParser(IParser<IModelCondition> conditionsParser, IParser<IReward> rewardParser)
    {
        _conditionsParser = conditionsParser;
        _rewardParser = rewardParser;
    }

    public GoalsTypeModel Parse(Attr data)
    {
        var goalParser = new GoalTypeModelParser(_conditionsParser, _rewardParser);
        var goalsList = data.AsList;
        var goals = new Dictionary<string, GoalTypeModel>();

        for(int index = 0; index < goalsList.Count; ++index)
        {
            var goal = goalParser.Parse(goalsList[index]);
            goals.Add(goal.Id, goal);
        }

        return new GoalsTypeModel(goals);
    }
}