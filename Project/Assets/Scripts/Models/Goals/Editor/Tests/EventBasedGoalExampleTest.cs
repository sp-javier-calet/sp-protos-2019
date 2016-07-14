using System;
using SocialPoint.Attributes;

public class EventBasedGoalExampleTest : IModelCondition
{
    const string AttrKeyValueKey = "value";

    string _eventName;
    int _valueExpected;
    int _repetitions;

    public int RequiredRepetitions
    {
        get
        {
            return 1;
        }
    }

    public EventBasedGoalExampleTest(string conditionName, int value)
    {
        _eventName = conditionName;
        _valueExpected = value;
    }

    public bool Matches(string name, Attr arguments)
    {
        return _eventName == name;
    }

    public bool ValidateEvent(PlayerModel playerModel, Attr eventData)
    {
        var value = eventData.AsDic[AttrKeyValueKey].AsValue.ToInt();

        return value == _valueExpected;
    }

    public bool ValidateModel(PlayerModel player)
    {
        return false;
    }
}