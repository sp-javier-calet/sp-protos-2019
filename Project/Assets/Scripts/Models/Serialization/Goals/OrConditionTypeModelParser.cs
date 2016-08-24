using SocialPoint.Attributes;
using System.Collections.Generic;

public class OrConditionTypeModelParser : ConditionContainerTypeModelParser
{
    #region IChildParser implementation

    const string NameValue = "or";

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
        return new OrConditionTypeModel(NameValue, ParseRepetitions(data), ParseConditions(data));
    }

    #endregion
}
