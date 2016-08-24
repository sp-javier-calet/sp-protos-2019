using SocialPoint.ScriptEvents;
using SocialPoint.Attributes;
using System;

public abstract class ConditionContainerTypeModel : IModelCondition
{
    IModelCondition[] _conditions;
    int _repetitions;

    public int RequiredRepetitions
    {
        get
        {
            return _repetitions;
        }
    }

    protected IModelCondition[] Conditions
    {
        get
        {
            return _conditions;
        }
    }

    public ConditionContainerTypeModel(string conditionName, int repetitions, IModelCondition[] conditions)
    {
        _conditions = conditions;
        _repetitions = repetitions;

        for(int index = 0; index < conditions.Length; ++index)
        {
            if(conditions[index].RequiredRepetitions > 1)
            {
                throw new Exception("Condition container cannot contain conditions with repetitions. Set the repetitions in the container itself or use a condition array");
            }
        }
    }

    public bool Matches(string name, Attr arguments)
    {
        for(int index = 0; index < _conditions.Length; ++index)
        {
            if(_conditions[index].Matches(name, arguments))
            {
                return true;
            }
        }

        return false;
    }

    public abstract bool ValidateEvent(PlayerModel playerModel, Attr eventData);

    public abstract bool ValidateModel(PlayerModel playerModel);
}
