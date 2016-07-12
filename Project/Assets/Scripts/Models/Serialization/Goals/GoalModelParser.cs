using System.Collections;
using SocialPoint.Attributes;
using System.Collections.Generic;

public class GoalModelParser : IParser<GoalModel>, ISerializer<GoalModel>
{
    const string AttrKeyGoalId = "id";
    const string AttrKeyGoalCompleted = "completed";
    const string AttrKeyGoalClaimed = "claimed";
    const string AttrKeyRepetitionsValidated = "repetitions_validated";

    ConfigModel _config;

    public GoalModelParser(ConfigModel config)
    {
        _config = config;
    }

    public GoalModel Parse(Attr data)
    {
        var goalId = data.AsDic[AttrKeyGoalId].ToString();

        var goalType = _config.Goals.GetGoalById(goalId);

        if(goalType == null)
        {
            return null;
        }

        var repetitionsValidatedList = data.AsDic[AttrKeyRepetitionsValidated].AsList;
        var repetitionsValidated = new List<int>();

        for(int index = 0; index < repetitionsValidatedList.Count; ++index)
        {
            repetitionsValidated.Add(repetitionsValidatedList[index].AsValue.ToInt());
        }

        return new GoalModel(
            id : goalId,
            completed: data.AsDic[AttrKeyGoalCompleted].AsValue.ToBool(),
            claimed : data.AsDic[AttrKeyGoalClaimed].AsValue.ToBool(),
            type : _config.Goals.GetGoalById(goalId),
            repetitionsValidated : repetitionsValidated
        );
    }

    public Attr Serialize(GoalModel goal)
    {
        var data = new AttrDic();

        data.SetValue(AttrKeyGoalId, goal.Id);
        data.SetValue(AttrKeyGoalCompleted, goal.Completed);
        data.SetValue(AttrKeyGoalClaimed, goal.Claimed);

        var repetitionsValidated = new AttrList();

        for(int index = 0; index < goal.RepetitionsValidated.Count; ++index)
        {
            repetitionsValidated.AddValue(goal.RepetitionsValidated[index]);
        }

        data.Set(AttrKeyRepetitionsValidated, repetitionsValidated);

        return data;
    }
}