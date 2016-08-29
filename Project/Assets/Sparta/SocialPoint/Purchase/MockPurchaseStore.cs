using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;

namespace SocialPoint.Purchase
{
    public sealed class MockPurchaseStore : IPurchaseStore
    {
        static int nextOrderId = 1;
        List<Product> _allProducts;
        List<Product> _productList;
        Product? _pendingPurchaseProduct;

        public int DelayUntilProductTransactionSecs{ get; set; }

        public bool MakeTransationsFail { get; set; }


        public MockPurchaseStore()
        {
            _allProducts = new List<Product>();
        }

        #region IPurchaseStore implementation

        public void Dispose()
        {
        }

        public event ProductsUpdatedDelegate ProductsUpdated;
        public event PurchaseUpdatedDelegate PurchaseUpdated;
        public event ValidatePurchaseDelegate ValidatePurchased;

        public ValidatePurchaseDelegate ValidatePurchase
        {
            set
            {
                ValidatePurchased += value;
            }
        }

        public ILoginData LoginData { get; set; }

        public void Setup(AttrDic settings)
        {
            //Implement if needed
        }

        public void LoadProducts(string[] productIds)
        {
            _productList = _allProducts.FindAll(p => productIds.Contains(p.Id)) ?? new List<Product>();
            _productList.Sort((p1, p2) => p1.Price.CompareTo(p2.Price));

            ProductsUpdated(LoadProductsState.Success, null);
        }

        public bool Purchase(string productId)
        {
            if(_productList == null)
            {
                Log.i("[MOCK] There are no products, call LoadProducts() first");
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                return false;
            }

            if(_pendingPurchaseProduct.HasValue)
            {
                Log.i("[MOCK] A transaction for product {0} is running and only one transaction is allowed ");
                return false;
            }
            Log.i("[MOCK] Buying product: " + productId);

            if(_productList.Exists(p => p.Id == productId))
            {
                _pendingPurchaseProduct = _productList.Find(p => p.Id == productId);
                PurchaseUpdated(PurchaseState.PurchaseStarted, productId);
                ProcessPendingTransaction();
                return true;
            }

            Log.i("[MOCK] product doesn't exist: " + productId);
            PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
            return false;
        }

        public bool IsPendingTransaction(string productId)
        {
            return _pendingPurchaseProduct.HasValue && _pendingPurchaseProduct.Value.Id == productId;
        }

        public void ProductPurchaseConfirmed(string transactionIdentifier)
        {
            Log.i("[MOCK] ProductPurchaseConfirmed: " + transactionIdentifier);

            if(!_pendingPurchaseProduct.HasValue)
            {
                Log.i("[MOCK] No pending product with id: " + transactionIdentifier);
                return;
            }

            PurchaseUpdated(PurchaseState.PurchaseFinished, transactionIdentifier);
            PurchaseUpdated(PurchaseState.PurchaseConsumed, transactionIdentifier);
            _pendingPurchaseProduct = null;
        }

        public void ForceFinishPendingTransactions()
        {
            if(!_pendingPurchaseProduct.HasValue)
            {
                return;
            }

            PurchaseUpdated(PurchaseState.PurchaseCanceled, _pendingPurchaseProduct.Value.Id);

            _pendingPurchaseProduct = null;
        }

        public bool HasProductsLoaded
        {
            get
            {
                return (_productList != null && _productList.Count > 0);
            }
        }

        public Product[] ProductList
        {
            get
            {
                return _productList.ToArray();
            }
        }

        #endregion

        internal void SetProductMockList(IEnumerable<Product> productMockList)
        {
            _allProducts = new List<Product>(productMockList) ?? new List<Product>();
        }

        void ProcessPendingTransaction()
        {
            if(!_pendingPurchaseProduct.HasValue)
            {
                return;
            }

            if(this.DelayUntilProductTransactionSecs > 0)
            {
                System.Threading.Thread.Sleep(this.DelayUntilProductTransactionSecs);
            }

            if(MakeTransationsFail)
            {
                PurchaseUpdated(PurchaseState.PurchaseFailed, _pendingPurchaseProduct.Value.Id);
            }
            else
            {
                PurchaseUpdated(PurchaseState.PurchaseFinished, _pendingPurchaseProduct.Value.Id);
                PurchaseUpdated(PurchaseState.PurchaseConsumed, _pendingPurchaseProduct.Value.Id);
                var data = new AttrDic();
                data.SetValue(Receipt.OrderIdKey, nextOrderId++);
                data.SetValue(Receipt.ProductIdKey, _pendingPurchaseProduct.Value.Id);
                data.SetValue(Receipt.PurchaseStateKey, string.Empty);
                data.SetValue(Receipt.StoreKey, "MockPurchase");

                // This is important to be null so no http request for a purchas confirmation against the SP servers
                // is sent when the InAppPurchaseManager handles this event.
                data.SetValue(Receipt.OriginalJsonKey, null);
                ValidatePurchased(new Receipt(data), (response) => {
                    Log.i("[MOCK] Purchase finished: " + response);
                });

            }

            _pendingPurchaseProduct = null;

        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            var handler = PurchaseUpdated;
            if(handler != null)
            {
                handler(state, productID);
            }
        }
    }

}
