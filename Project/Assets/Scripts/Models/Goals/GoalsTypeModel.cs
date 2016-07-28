using System;
using System.Collections;
using System.Collections.Generic;

public class GoalsTypeModel : IDisposable, IEnumerable<KeyValuePair<string, GoalTypeModel>>
{
    Dictionary<string, GoalTypeModel> _goals;

    public GoalsTypeModel(Dictionary<string, GoalTypeModel> goals)
    {
        _goals = goals;
    }

    public void Dispose()
    {
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
    }

    public GoalTypeModel GetGoalById(string goalId)
    {
        GoalTypeModel goal;

        if(_goals.TryGetValue(goalId, out goal))
        {
            return goal;
        }

        return null;
    }

    #region IEnumerable implementation

    public Dictionary<string, GoalTypeModel>.Enumerator GetEnumerator()
    {
        return _goals.GetEnumerator();
    }

    IEnumerator<KeyValuePair<string, GoalTypeModel>> IEnumerable<KeyValuePair<string, GoalTypeModel>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}
