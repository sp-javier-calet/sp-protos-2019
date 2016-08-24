using System.Collections.Generic;
using System;

public class GoalModel : IDisposable
{
    public string Id{ get; private set; }

    public bool Completed
    {
        get
        {
            return _completed;
        }
        private set
        {
            if(_completed)
            {
                throw new GoalCompletionException(Id);
            }

            _completed = value;
        }
    }

    public bool Claimed{ get; private set; }

    public List<int> RepetitionsValidated{ get; private set; }

    public GoalTypeModel TypeModel{ get; private set; }

    bool _completed;

    public GoalModel(string id, bool completed, bool claimed, GoalTypeModel type, List<int> repetitionsValidated = null)
    {
        Id = id;
        Claimed = claimed;
        Completed = completed;
        TypeModel = type;

        RepetitionsValidated = repetitionsValidated ?? new List<int>();

        for(int index = RepetitionsValidated.Count; index < TypeModel.Conditions.Count; ++index)
        {
            RepetitionsValidated.Insert(index, 0);
        }

        CheckCompleted();
    }

    public void OnRepetitionValidated(int repetitionIndex)
    {
        if(repetitionIndex >= TypeModel.Conditions.Count)
        {
            throw new Exception(string.Format("Out of bounds trying to validate the condition {0} of the goal {1}", repetitionIndex, Id));
        }

        RepetitionsValidated[repetitionIndex]++;

        CheckCompleted();
    }

    void CheckCompleted()
    {
        if(!Completed)
        {
            bool notEnoughRepetitionsFound = false;

            for(int index = 0; index < TypeModel.Conditions.Count && !notEnoughRepetitionsFound; ++index)
            {
                int requiredRepetitions = TypeModel.Conditions[index].RequiredRepetitions;

                if(requiredRepetitions == 0)
                {
                    continue;
                }
                else if(RepetitionsValidated[index] < requiredRepetitions)
                {
                    notEnoughRepetitionsFound = true;
                    break;
                }
            }

            Completed = !notEnoughRepetitionsFound;
        }
    }

    public bool Claim(PlayerModel playerModel)
    {
        if(!Completed)
        {
            throw new GoalNotCompletedException(Id);
        }
        else if(Claimed)
        {
            throw new GoalAlreadyClaimedException(Id);
        }

        var error = TypeModel.Reward.Obtain(playerModel);

        Claimed = error == null;

        return Claimed;
    }

    public void Dispose()
    {
        if(RepetitionsValidated != null)
        {
            RepetitionsValidated.Clear();

            RepetitionsValidated = null;
        }
    }
}
