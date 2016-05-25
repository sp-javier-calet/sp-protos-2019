
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


    PurchaseCostFactory _purchaseCostFactory;

    public PurchaseCostParser(PurchaseCostFactory purchaseCostFactory)
    {
        _purchaseCostFactory = purchaseCostFactory;
    }

    public ICost Parse(Attr data)
    {
        return _purchaseCostFactory.CreatePurchaseCost(data.AsValue.ToString());
    }

    #endregion
}
