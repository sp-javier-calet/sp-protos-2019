using SocialPoint.Attributes;

namespace SocialPoint.ServerSync
{
    public sealed class PurchaseCommand : Command
    {
        static readonly string TypeName = "purchase";
        static readonly string ReceiptKey = "receipt";
        static readonly string GatewayKey = "gateway";

        public PurchaseCommand(string receipt, string store) : base(TypeName)
        {
            Atomic = false;
            var args = new AttrDic();
            args.SetValue(ReceiptKey, receipt);
            args.SetValue(GatewayKey, store);
            Arguments = args;
        }
    }

}
