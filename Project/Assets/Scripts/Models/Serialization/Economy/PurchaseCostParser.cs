
using SocialPoint.Attributes;

using Zenject;

public class PurchaseCostParser : IChildParser<ICost>
{

    #region IChildParser implementation

    const string NameValue = "purchase";

    public string Name
    {
        get
        {
            return NameValue;
        }
    }

    public FamilyParser<ICost> Parent{ set { } }

    [Inject]
    IFactory<string,PurchaseCost> _purchaseCostFactory;

    public ICost Parse(Attr data)
    {
        return _purchaseCostFactory.Create(data.AsValue.ToString());
    }

    #endregion
}
