using SocialPoint.Base;
using System;

public class StoreItem
{
    public string Id{ get; private set; }

    public virtual string Name { get; private set; }

    public virtual string Description { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is hidden by json.
    /// </summary>
    /// <value><c>true</c> if this instance is hidden; otherwise, <c>false</c>.</value>
    public bool IsHidden{ get; private set; }

    /// <summary>
    /// Gets a value indicating whether this instance is allowed depending game rules (ex:max building reached).
    /// </summary>
    /// <value><c>true</c> if this instance is allowed; otherwise, <c>false</c>.</value>
    public virtual bool IsAllowed { get { return true; } }

    /// <summary>
    /// Gets a value indicating whether this instance is unlocked depending game rules (ex: min level reached)..
    /// </summary>
    /// <value><c>true</c> if this instance is unlocked; otherwise, <c>false</c>.</value>
    public virtual bool IsUnlocked { get { return true; } }

    public ICost Cost;
    public IReward Reward;

    public StoreItem(string id, string name, string description, bool hidden, ICost cost, IReward reward)
    {
        Id = id;
        Name = name;
        Description = description;
        IsHidden = hidden;
        Cost = cost;
        Reward = reward;
    }

    public void Purchase(Action<Error> finished = null)
    {
        if(Cost == null)
        {
            OnPurchaseCostSpent(null, finished);
        }
        else
        {
            Cost.Spend((err) => OnPurchaseCostSpent(err, finished));
        }
    }

    void OnPurchaseCostSpent(Error err, Action<Error> finished)
    {
        if(!Error.IsNullOrEmpty(err))
        {
            if(finished != null)
            {
                finished(err);
            }
            return;
        }
        if(Reward != null)
        {
            err = Reward.Obtain();
        }
        if(finished != null)
        {
            finished(err);
        }
    }

    public override string ToString()
    {
        return string.Format("[StoreItem: Id={0}, Name={1}, Description={2}, IsHidden={3}, IsAllowed={4}, IsUnlocked={5}]", Id, Name, Description, IsHidden, IsAllowed, IsUnlocked);
    }
}

