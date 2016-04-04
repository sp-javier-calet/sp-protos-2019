
using SocialPoint.Attributes;
using SocialPoint.Purchase;

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

    IGamePurchaseStore _store;

    public PurchaseCostParser(IGamePurchaseStore store)
    {
        _store = store;
    }

    public ICost Parse(Attr data)
    {
        return new PurchaseCost(data.AsValue.ToString(), _store);
    }

    #endregion
}
