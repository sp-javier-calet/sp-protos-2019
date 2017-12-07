using System.Collections.Generic;
using System;
using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using UnityEngine;

public class GoalsModel : IDisposable
{
    Dictionary<string, GoalModel> _goals;
    GoalsTypeModel _goalsConfig;
    IScriptEventProcessor _processor;
    PlayerModel _playerModel;
    Dictionary<IModelCondition, Action<string, Attr>> _listenersByCondition;

    public IEnumerator<KeyValuePair<string, GoalModel>> Goals
    {
        get
        {
            return _goals.GetEnumerator();
        }
    }

    public event Action<GoalModel> GoalCompleted;
    public event Action<GoalModel> GoalClaimed;

    public GoalsModel Init(Dictionary<string, GoalModel> goals, GoalsTypeModel goalsConfig, IScriptEventProcessor processor, PlayerModel playerModel)
    {
        Cleanup();

        _listenersByCondition = new Dictionary<IModelCondition, Action<string, Attr>>();

        _goals = goals;
        _goalsConfig = goalsConfig;
        _processor = processor;
        _playerModel = playerModel;

        _playerModel.Initialized += Initialize;

        return this;
    }

    void Initialize(PlayerModel playerModel)
    {
        if(_goalsConfig != null && _processor != null)
        {
            var goalsEnumerator = _goalsConfig.GetEnumerator();

            while(goalsEnumerator.MoveNext())
            {
                var goal = goalsEnumerator.Current.Value;
                string goaldId = goalsEnumerator.Current.Key;

                GoalModel goalModel = null;

                if(_goals.TryGetValue(goaldId, out goalModel) && goalModel.Completed)
                {
                    continue;
                }

                if(!ValidateGoalCompletion(goal))
                {
                    RegisterGoalConditions(goal);
                }
            }

            goalsEnumerator.Dispose();
        }
    }

    public GoalModel GetGoalById(string goalId)
    {
        GoalModel goalModel;

        if(_goals.TryGetValue(goalId, out goalModel))
        {
            return goalModel;
        }

        return null;
    }

    public float GetGoalProgress(GoalModel goalModel)
    {
        if(goalModel == null)
        {
            return 0.0f;
        }
        else if(goalModel.Completed)
        {
            return 1.0f;
        }

        int requiredRepetitions = 0;

        for(int index = 0; index < goalModel.TypeModel.Conditions.Count; ++index)
        {
            requiredRepetitions += goalModel.TypeModel.Conditions[index].RequiredRepetitions;
        }

        int validatedRepetitions = 0;

        for(int index = 0; index < goalModel.RepetitionsValidated.Count; ++index)
        {
            validatedRepetitions += goalModel.RepetitionsValidated[index];
        }

        return Math.Min((float)validatedRepetitions / (float)requiredRepetitions, 1.0f);
    }

    void OnGoalMatched(GoalTypeModel goalType, int goalIndex, Attr eventData)
    {
        var goalCondition = goalType.Conditions[goalIndex];

        if(goalCondition.ValidateEvent(_playerModel, eventData))
        {
            var goal = ObtainGoalModel(goalType);

            goal.OnRepetitionValidated(goalIndex);

            CheckGoalCompleted(goal);

            if(goal.Completed)
            {
                UnregisterGoalConditions(goalType);
            }
        }
    }

    void RegisterGoalConditions(GoalTypeModel goal)
    {
        for(int index = 0; index < goal.Conditions.Count; ++index)
        {
            var matchedGoal = goal;
            var goalIndex = index;

            var condition = goal.Conditions[index];
            Action<string, Attr> listener = (name, attributes) => OnGoalMatched(matchedGoal, goalIndex, attributes);

            _listenersByCondition.Add(condition, listener);
            _processor.RegisterHandler(condition, listener);
        }
    }

    void UnregisterGoalConditions(GoalTypeModel goal)
    {
        for(int index = 0; index < goal.Conditions.Count; ++index)
        {
            var condition = goal.Conditions[index];
            Action<string, Attr> listener = null;

            if(_listenersByCondition.TryGetValue(condition, out listener))
            {
                _processor.UnregisterHandler(listener);
                _listenersByCondition.Remove(condition);
            }
        }
    }

    GoalModel ObtainGoalModel(GoalTypeModel goalType)
    {
        var goalId = goalType.Id;
        GoalModel goal = GetGoalById(goalId);

        if(goal == null)
        {
            goal = new GoalModel(goalId, false, false, goalType);
            _goals.Add(goalId, goal);
        }

        return goal;
    }

    bool ValidateGoalCompletion(GoalTypeModel goalType)
    {
        GoalModel goal = null;

        for(int index = 0; index < goalType.Conditions.Count; ++index)
        {
            if(goalType.Conditions[index].ValidateModel(_playerModel))
            {
                if(goal == null)
                {
                    goal = ObtainGoalModel(goalType);
                }

                goal.OnRepetitionValidated(index);
            }
        }

        if(goal != null)
        {
            CheckGoalCompleted(goal);

            return goal.Completed;
        }

        return false;
    }

    void CheckGoalCompleted(GoalModel goal)
    {
        if(goal.Completed)
        {
            if(GoalCompleted != null)
            {
                GoalCompleted(goal);
            }
        }
    }

    public void ClaimGoal(string goalId)
    {
        GoalModel goalModel = GetGoalById(goalId);

        if(goalModel == null)
        {
            throw new GoalNotFoundException(goalId);
        }
            
        if(goalModel.Claim(_playerModel) && GoalClaimed != null)
        {
            GoalClaimed(goalModel);
        }
    }

    void Cleanup()
    {
        if(_playerModel != null)
        {
            _playerModel.Initialized -= Initialize;
            _playerModel = null;
        }

        if(_goals != null)
        {
            var goalsEnumerator = _goals.GetEnumerator();

            while(goalsEnumerator.MoveNext())
            {
                goalsEnumerator.Current.Value.Dispose();
            }

            goalsEnumerator.Dispose();

            _goals.Clear();

            _goals = null;
        }

        if(_listenersByCondition != null)
        {
            if(_processor != null)
            {
                var listenerEnumerator = _listenersByCondition.GetEnumerator();

                while(listenerEnumerator.MoveNext())
                {
                    _processor.UnregisterHandler(listenerEnumerator.Current.Value);
                }

                listenerEnumerator.Dispose();
            }

            _listenersByCondition.Clear();
            _listenersByCondition = null;
        }
    }

    public void Dispose()
    {
        Cleanup();
    }
}
