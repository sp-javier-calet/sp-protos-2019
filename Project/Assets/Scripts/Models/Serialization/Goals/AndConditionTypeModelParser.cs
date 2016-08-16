using SocialPoint.Attributes;
using System.Collections.Generic;

public class AndConditionTypeModelParser : ConditionContainerTypeModelParser
{
    #region IChildParser implementation

    const string NameValue = "and";

    public override string Name
    {
        get
        {
            return NameValue;
        }
    }

    public override FamilyParser<IModelCondition> Parent{ get; set; }

    public override IModelCondition Parse(Attr data)
    {
        return new AndConditionTypeModel(NameValue, ParseRepetitions(data), ParseConditions(data));
    }

    #endregion
}
