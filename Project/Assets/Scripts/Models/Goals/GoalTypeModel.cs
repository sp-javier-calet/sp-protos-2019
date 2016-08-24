using System;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using System.Collections.Generic;

public class GoalTypeModel : IDisposable
{
    public string Id { get; private set; }

    public List<IModelCondition> Conditions { get; private set; }

    public Attr Tutorial { get; private set; }

    public IReward Reward { get; private set; }

    public GoalTypeModel(string id, List<IModelCondition> conditions, Attr tutorial, IReward reward)
    {
        Id = id;
        Conditions = conditions;
        Tutorial = tutorial;
        Reward = reward;
    }

    public void Dispose()
    {
        if(Conditions != null)
        {
            Conditions.Clear();

            Conditions = null;
        }
    }
}