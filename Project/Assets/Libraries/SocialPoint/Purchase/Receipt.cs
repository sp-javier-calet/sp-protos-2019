using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public struct Receipt
    {
        public string OrderId{ get; private set; }

        public string ProductId{ get; private set; }

        public PurchaseState State{ get; private set; }

        public string OriginalJson{ get; private set; }

        public string Store { get; private set; }
        //TODO: add specific attributes for different platforms
        public string DataSignature { get; private set; }

        public static readonly string OrderIdKey = "OrderId";
        public static readonly string ProductIdKey = "ProductId";
        public static readonly string PurchaseStateKey = "PurchaseState";
        public static readonly string OriginalJsonKey = "OriginalJson";
        public static readonly string StoreKey = "Store";
        public static readonly string DataSignatureKey = "DataSignature";

        public Receipt(Attr data) : this()
        {
            var dataDic = data.AssertDic;
            OrderId = dataDic.GetValue(OrderIdKey).ToString();
            ProductId = dataDic.GetValue(ProductIdKey).ToString();
            State = (PurchaseState)dataDic.GetValue(PurchaseStateKey).ToInt();
            OriginalJson = dataDic.GetValue(OriginalJsonKey).ToString();
            Store = dataDic.GetValue(StoreKey).ToString();
            DataSignature = dataDic.GetValue(DataSignatureKey).ToString();
        }

        public Attr ToAttr()
        {
            var data = new AttrDic();
            data.SetValue(OrderIdKey, OrderId);
            data.SetValue(ProductIdKey, ProductId);
            data.SetValue(PurchaseStateKey, (int)State);
            data.SetValue(OriginalJsonKey, OriginalJson);
            data.SetValue(StoreKey, Store);
            data.SetValue(DataSignatureKey, DataSignature);
            return data;
        }

        public override string ToString()
        {
            return string.Format("[Receipt details: Id = {0}, Product = {1}, State = {2}, original = {3}, store = {4}, dataSignature = {5}]",
                                 this.OrderId, this.ProductId, this.State.ToString(), this.OriginalJson, this.Store, this.DataSignature);
        }
    }
}
