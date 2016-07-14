using System;

public class GoalException : Exception
{
    protected string _goalId;
    protected string _message;

    public GoalException(string goalId, string message)
    {
        _goalId = goalId;
        _message = message;
    }

    public override string ToString()
    {
        return string.Format("{0} - Error in goal \"{1}\": {2}", GetType(), _goalId, _message);
    }
}

public class GoalCompletionException : GoalException
{
    public GoalCompletionException(string goalId) : base(goalId, "It's forbidden to modify the completion of a goal when is already completed")
    {
    }
}

public class GoalNotCompletedException : GoalException
{
    public GoalNotCompletedException(string goalId) : base(goalId, "Tried to claim an uncompleted goal")
    {
    }
}

public class GoalAlreadyClaimedException : GoalException
{
    public GoalAlreadyClaimedException(string goalId) : base(goalId, "Tried to claim an already claimed goal")
    {
    }
}

public class GoalNotFoundException : GoalException
{
    public GoalNotFoundException(string goalId) : base(goalId, "Goal not found")
    {
    }
}
