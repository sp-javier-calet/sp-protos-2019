using SocialPoint.Attributes;
using System.Collections.Generic;

public abstract class ConditionContainerTypeModelParser : IChildParser<IModelCondition>
{
    const string AttrKeyRepetitions = "repetitions";
    const string AttrKeyConditions = "conditions";

    #region IChildParser implementation

    public abstract string Name{ get; }

    public abstract FamilyParser<IModelCondition> Parent{ get; set; }

    public abstract IModelCondition Parse(Attr data);

    public int ParseRepetitions(Attr data)
    {
        return data.AsDic.ContainsKey(AttrKeyRepetitions) ? data.AsDic[AttrKeyRepetitions].AsValue.ToInt() : 1;
    }

    public IModelCondition[] ParseConditions(Attr data)
    {
        var conditionsList = data.AsDic[AttrKeyConditions].AsList;
        var conditions = new IModelCondition[conditionsList.Count];

        for(int index = 0; index < conditionsList.Count; ++index)
        {
            conditions[index] = Parent.Parse(conditionsList[index]);
        }

        return conditions;
    }

    #endregion
}
