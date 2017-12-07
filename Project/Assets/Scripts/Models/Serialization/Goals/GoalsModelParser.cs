using SocialPoint.Attributes;
using System.Collections.Generic;
using SocialPoint.ScriptEvents;
using System;

public class GoalsModelParser : IAttrObjParser<GoalsModel>, IAttrObjSerializer<GoalsModel>
{
    GoalsModel _goals;
    ConfigModel _config;
    GoalModelParser _goalParser;
    IScriptEventProcessor _scriptEventDispatcher;
    PlayerModel _playerModel;

    public GoalsModelParser(GoalsModel goals, ConfigModel config, IScriptEventProcessor scriptEventDispatcher, PlayerModel playerModel)
    {
        _goals = goals;
        _config = config;
        _goalParser = new GoalModelParser(config);
        _scriptEventDispatcher = scriptEventDispatcher;
        _playerModel = playerModel;
    }

    public GoalsModel Parse(Attr data)
    {
        var goalsList = data.AsList;
        var goals = new Dictionary<string, GoalModel>();

        for(int index = 0; index < goalsList.Count; ++index)
        {
            var goal = _goalParser.Parse(goalsList[index]);

            if(goal != null)
            {
                goals.Add(goal.Id, goal);
            }
        }

        return _goals.Init(goals, _config.Goals, _scriptEventDispatcher, _playerModel);
    }

    public Attr Serialize(GoalsModel goals)
    {
        var nodes = new AttrList();
        var goalsEnumerator = goals.Goals;

        while(goalsEnumerator.MoveNext())
        {
            nodes.Add(_goalParser.Serialize(goalsEnumerator.Current.Value));
        }

        goalsEnumerator.Dispose();

        return nodes;
    }
}