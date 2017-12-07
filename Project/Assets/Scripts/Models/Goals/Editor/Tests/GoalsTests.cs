using System.Collections;
using NUnit.Framework;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;
using SocialPoint.Attributes;
using System;

[TestFixture]
[Category("SocialPoint.Goals")]
public class GoalsTests
{
    PlayerModel _playerModel;
    IScriptEventProcessor _scriptEventDispatcher;
    GoalsTypeModel _goalsTypeModel;
    Dictionary<string, GoalModel> _goalsModel;

    const string _eventGoalName = "event_goal_test";
    const string _modelGoalName = "model_goal_test";

    int _goalsCompletedByCallback;

    [SetUp]
    public void SetUp()
    {
        _playerModel = new PlayerModel();
        _scriptEventDispatcher = new ScriptEventProcessor();

        var goalsConfig = new Dictionary<string, GoalTypeModel>();
        _goalsModel = new Dictionary<string, GoalModel>();

        IncludeEventBasedGoal(goalsConfig, _goalsModel);

        IncludeModelBasedGoal(goalsConfig, _goalsModel);

        _goalsTypeModel = new GoalsTypeModel(goalsConfig);
    }

    void InitPlayer()
    {
        _playerModel.Goals.Init(_goalsModel, _goalsTypeModel, _scriptEventDispatcher, _playerModel);
        _playerModel.Init(1, new ResourcePool());
    }

    void IncludeEventBasedGoal(Dictionary<string, GoalTypeModel> goalsConfig, Dictionary<string, GoalModel> goalsModel)
    {
        var conditionOne = new EventBasedGoalExampleTest("event_condition_example", 6);
        var conditionTwo = new EventBasedGoalExampleTest("event_condition_example", 2);

        var conditions = new IModelCondition[2];
        conditions[0] = conditionOne;
        conditions[1] = conditionTwo;

        var repetitions = 2;

        var orCondition = new OrConditionTypeModel("or", repetitions, conditions);
        var conditionList = new List<IModelCondition>();
        conditionList.Add(orCondition);

        var goalConfig = new GoalTypeModel(_eventGoalName, conditionList, null, null);

        goalsConfig.Add(goalConfig.Id, goalConfig);

        // start with one repetition already completed
        var repetitionsCompletedSoFar = new List<int>();
        repetitionsCompletedSoFar.Add(1);

        var goalModel = new GoalModel(_eventGoalName, false, false, goalConfig, repetitionsCompletedSoFar);
        goalsModel.Add(goalModel.Id, goalModel);
    }

    void IncludeModelBasedGoal(Dictionary<string, GoalTypeModel> goalsConfig, Dictionary<string, GoalModel> goalsModel)
    {
        var requiredResources = new ResourcePool();

        requiredResources["g"] = 1000;
        var condition = new ModelBasedGoalExampleTest("model_condition_example", requiredResources);

        var conditionList = new List<IModelCondition>();
        conditionList.Add(condition);

        var goalConfig = new GoalTypeModel(_modelGoalName, conditionList, null, null);

        goalsConfig.Add(goalConfig.Id, goalConfig);

        var goalModel = new GoalModel(_modelGoalName, false, false, goalConfig);
        goalsModel.Add(goalModel.Id, goalModel);
    }

    [Test]
    public void EventBasedGoalAccomplishment()
    {
        InitPlayer();

        var attr = new AttrDic();
        attr.SetValue("value", 3);

        Assert.AreEqual(CompletedGoals, 0);

        //wrong number. this won't complete any repetition
        _scriptEventDispatcher.Process("event_condition_example", attr);

        Assert.AreEqual(CompletedGoals, 0);

        CompleteEventBasedGoal();

        Assert.AreEqual(CompletedGoals, 1);
    }

    void CompleteEventBasedGoal()
    {
        var attr = new AttrDic();
        attr.SetValue("value", 6);

        //right number. this will complete another repetition.
        //we had one repetition completed, and with two repetitions needed, now we have completed the goal
        _scriptEventDispatcher.Process("event_condition_example", attr);
    }

    [Test]
    public void ModelBasedGoalAccomplishment()
    {
        InitPlayer();

        Assert.AreEqual(CompletedGoals, 0);

        //the required resource is gold, so this won't complete the goal
        var resources = new ResourcePool();
        resources["f"] = 1000;

        AddResourcesToPlayer(resources);

        Assert.AreEqual(CompletedGoals, 0);

        //you need more resources than this, so this won't complete the goal
        resources = new ResourcePool();
        resources["g"] = 500;

        AddResourcesToPlayer(resources);

        Assert.AreEqual(CompletedGoals, 0);

        AddResourcesToPlayer(resources);

        Assert.AreEqual(CompletedGoals, 1);
    }

    void AddResourcesToPlayer(ResourcePool resources)
    {
        _playerModel.Resources.Add(resources);
        _scriptEventDispatcher.Process("model_condition_example", null);
    }

    [Test]
    public void GoalCompletedCallback()
    {
        InitPlayer();

        _playerModel.Goals.GoalCompleted += OnGoalCompleted;

        CompleteEventBasedGoal();

        Assert.AreEqual(_goalsCompletedByCallback, CompletedGoals);

        var resources = new ResourcePool();
        resources["g"] = 1000;

        AddResourcesToPlayer(resources);

        Assert.AreEqual(_goalsCompletedByCallback, CompletedGoals);

        _playerModel.Goals.GoalCompleted -= OnGoalCompleted;
    }

    void OnGoalCompleted(GoalModel goalCompleted)
    {
        _goalsCompletedByCallback++;
    }

    [Test]
    public void GoalCompletedWhenPlayerInitializes()
    {
        _playerModel.Goals.Init(_goalsModel, _goalsTypeModel, _scriptEventDispatcher, _playerModel);

        Assert.AreEqual(CompletedGoals, 0);

        var resources = new ResourcePool();
        resources["g"] = 1000;
        _playerModel.Init(1, resources);

        Assert.AreEqual(CompletedGoals, 1);
    }

    int CompletedGoals
    {
        get
        {
            int completedGoals = 0;

            var goals = _playerModel.Goals.Goals;

            while(goals.MoveNext())
            {
                if(goals.Current.Value.Completed)
                {
                    completedGoals++;
                }
            }

            goals.Dispose();

            return completedGoals;
        }
    }
}
