using System;
using SocialPoint.Attributes;

public class ModelBasedGoalExampleTest : IModelCondition
{
    string _eventName;
    ResourcePool _amountOfResourcesRequired;

    public int RequiredRepetitions
    {
        get
        {
            return 1;
        }
    }

    public ModelBasedGoalExampleTest(string conditionName, ResourcePool amountOfResourcesRequired)
    {
        _eventName = conditionName;
        _amountOfResourcesRequired = amountOfResourcesRequired;
    }

    public bool Matches(string name, Attr arguments)
    {
        return _eventName == name;
    }

    public bool ValidateEvent(PlayerModel playerModel, Attr eventData)
    {
        return ValidateModel(playerModel);
    }

    public bool ValidateModel(PlayerModel player)
    {
        var playerResources = player.Resources;

        var resourcesRequiredEnum = _amountOfResourcesRequired.GetEnumerator();

        bool notEnoughResourcesFound = false;

        while(resourcesRequiredEnum.MoveNext())
        {
            var playerResource = playerResources[resourcesRequiredEnum.Current.Key];

            if(playerResource < resourcesRequiredEnum.Current.Value)
            {
                notEnoughResourcesFound = true;
                break;
            }
        }

        resourcesRequiredEnum.Dispose();

        return !notEnoughResourcesFound;
    }
}

