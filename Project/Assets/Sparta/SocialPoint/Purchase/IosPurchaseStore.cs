#if UNITY_IOS
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using UnityEngine;
#endif

namespace SocialPoint.Purchase
{
    public class IosPurchaseStore
    #if UNITY_IOS
        : IPurchaseStore
    #endif
    {
        #if UNITY_IOS
        List<Product> _products;
        string _purchasingProduct;
        List<Receipt> _pendingPurchases;

        #region IPurchaseStore implementationcategoryModel

        public event ProductsUpdatedDelegate ProductsUpdated = delegate {};

        public event PurchaseUpdatedDelegate PurchaseUpdated = delegate {};

        ValidatePurchaseDelegate _validatePurchase;

        GetUserIdDelegate _getUserId;

        private delegate void OnFinishedPendingPurchaseDelegate();

        public ValidatePurchaseDelegate ValidatePurchase
        {
            set
            {
                if(_validatePurchase != null && value != null)
                {
                    throw new Exception("only one callback allowed!");
                }
                _validatePurchase = value;
            }
        }

        public GetUserIdDelegate GetUserId
        {
            set
            {
                _getUserId = value;
            }
        }

        public void Setup(AttrDic settings)
        {
            PlatformPuchaseSettings.SetBoolSetting(settings, 
                PlatformPuchaseSettings.IOSUseDetailedLogKey, 
                IosStoreBinding.EnableHighDetailLogs);
            
            PlatformPuchaseSettings.SetBoolSetting(settings, 
                PlatformPuchaseSettings.IOSUseAppReceiptKey, 
                IosStoreBinding.SetUseAppReceipt);
            
            PlatformPuchaseSettings.SetBoolSetting(settings, 
                PlatformPuchaseSettings.IOSSendTransactionUpdateEventsKey, 
                IosStoreBinding.SetShouldSendTransactionUpdateEvents);
        }

        [System.Diagnostics.Conditional("DEBUG_SPPURCHASE")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("IosPurchaseStore {0}", msg));
        }

        public void LoadProducts(string[] productIds)
        {
            DebugLog("requesting products");
            IosStoreBinding.RequestProductData(productIds);
        }

        public bool Purchase(string productId)
        {
            if(_products == null)
            {
                DebugLog("there are no products, load them first");
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                return false;
            }
            DebugLog("buying product: " + productId);
            if(_products.Exists(p => p.Id == productId))
            {
                if(_getUserId != null)
                {
                    IosStoreBinding.SetApplicationUsername(CryptographyUtils.GetHashSha256(_getUserId().ToString()));
                    IosStoreBinding.PurchaseProduct(productId);
                    _purchasingProduct = productId;
                    PurchaseUpdated(PurchaseState.PurchaseStarted, productId);
                    return true;
                }
                else
                {
                    PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                    string errorMessage = "An Application Username must be set before attempting to purchase. The game must provide a delegate through the SocialPointPurchaseStore.GetUserId setter.";
                    Debug.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                DebugLog("product doesn't exist: " + productId);
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                return false;
            }
        }

        public void ForceFinishPendingTransactions()
        {
            DebugLog("ForceFinishPendingTransactions");
            IosStoreBinding.ForceFinishPendingTransactions();
        }

        public bool HasProductsLoaded
        {
            get
            {
                return (_products != null && _products.Count > 0);
            }
        }

        public Product[] ProductList
        {
            get
            {
                return _products.ToArray();
            }
        }

        

        #endregion

        
        

        #region IDisposable implementation

        
        virtual public void Dispose()
        {
            UnregisterEvents();
        }

        

        #endregion

        
        public IosPurchaseStore()
        {
            if(Application.platform != RuntimePlatform.IPhonePlayer)
            {
                throw new NotImplementedException("IosPurchaseStore only works on iOS");
            }

            IosStoreManager.ProductListReceivedEvent += ProductListReceived;
            IosStoreManager.ProductListRequestFailedEvent += ProductListFailed;
            IosStoreManager.PurchaseFailedEvent += PurchaseFailed;
            IosStoreManager.PurchaseCancelledEvent += PurchaseCanceled;
            IosStoreManager.PurchaseSuccessfulEvent += PurchaseFinished;
            IosStoreManager.TransactionUpdatedEvent += TransactionUpdated;
        }

        void ProductListReceived(List<IosStoreProduct> products)
        {
            _products = new List<Product>();
            DebugLog("received total products: " + products.Count);
            try
            {
                for(int i = 0, productsCount = products.Count; i < productsCount; i++)
                {
                    IosStoreProduct product = products[i];
                    var parsedProduct = new Product(product.ProductIdentifier, product.Title, float.Parse(product.Price), product.CurrencySymbol, product.FormattedPrice);
                    DebugLog(product.ToString());
                    _products.Add(parsedProduct);
                }

            }
            catch(Exception ex)
            {
                DebugLog("parsing went wrong");
                ProductsUpdated(LoadProductsState.Error, new Error(ex.Message));
            }

            _products.Sort((Product p1, Product p2) => p1.Price.CompareTo(p2.Price));
            DebugLog("products sorted");
            ProductsUpdated(LoadProductsState.Success);
        }

        void ProductListFailed(Error error)
        {
            DebugLog("ProductListFailed " + error);
        }

        void FinishPendingPurchase(Receipt receipt, OnFinishedPendingPurchaseDelegate OnFinishedPendingPurchase = null)
        {
            if(_validatePurchase != null)
            {
                DebugLog("ProductPurchaseAwaitingConfirmation: " + receipt.ToString());
                _validatePurchase(receipt, (response) => {
                    DebugLog("response given to IosPurchaseStore: " + response.ToString() + " for transaction: " + receipt.OrderId);
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        IosStoreBinding.FinishPendingTransaction(receipt.OrderId);
                        PurchaseUpdated(PurchaseState.PurchaseConsumed, receipt.ProductId);
                        if(_pendingPurchases != null)
                        {
                            _pendingPurchases.Remove(receipt);
                        }
                        if(OnFinishedPendingPurchase != null)
                        {
                            OnFinishedPendingPurchase();
                        }
                    }
                    //itunes api can only confirm a purchase(can't cancel) so we call nothing unless our backend says it's complete.
                });
            }
        }

        void FinishAllPendingPurchases()
        {
            if(_pendingPurchases != null && _pendingPurchases.Count > 0)
            {
                Receipt receipt = _pendingPurchases[0];
                FinishPendingPurchase(receipt, FinishAllPendingPurchases);//Call again when this purchase is removed
            }
            else
            {
                DebugLog("All pending purchases finished");
            }
        }

        void PurchaseFailed(Error error)
        {
            DebugLog("PurchaseFailed " + error);
            //_purchasingProduct may be uninitialized if the event comes when loading old (not consumed) transactions when the store is initialized
            if(!String.IsNullOrEmpty(_purchasingProduct))
            {
                PurchaseUpdated(PurchaseState.PurchaseFailed, _purchasingProduct);
            }
        }

        void PurchaseCanceled(Error error)
        {
            DebugLog("PurchaseCanceled " + error);
            //_purchasingProduct may be uninitialized if the event comes when loading old (not consumed) transactions when the store is initialized
            if(!String.IsNullOrEmpty(_purchasingProduct))
            {
                PurchaseUpdated(PurchaseState.PurchaseCanceled, _purchasingProduct);
            }
        }

        void PurchaseFinished(IosStoreTransaction transaction)
        {
            DebugLog("Purchase has finished: " + transaction.TransactionIdentifier);
            PurchaseUpdated(PurchaseState.PurchaseFinished, transaction.ProductIdentifier);

            //Validate with backend after purchase was successful
            ProductPurchaseAwaitingConfirmation(transaction);
        }

        void ProductPurchaseAwaitingConfirmation(IosStoreTransaction transaction)
        {
            if(_pendingPurchases == null)
            {
                _pendingPurchases = new List<Receipt>();
            }

            Receipt receipt = GetReceiptFromTransaction(transaction);
            _pendingPurchases.Add(receipt);

            if(_products != null && _products.Count > 0)
            {
                FinishPendingPurchase(receipt);
            }
        }

        void TransactionUpdated(IosStoreTransaction transaction)
        {
            DebugLog("Transaction Updated: " + transaction.TransactionState);
        }

        static Receipt GetReceiptFromTransaction(IosStoreTransaction transaction)
        {
            var data = new AttrDic();
            data.SetValue(Receipt.OrderIdKey, transaction.TransactionIdentifier);
            data.SetValue(Receipt.ProductIdKey, transaction.ProductIdentifier);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, transaction.Base64EncodedTransactionReceipt);
            data.SetValue(Receipt.StoreKey, "itunes");
            return new Receipt(data);
        }

        void UnregisterEvents()
        {
            IosStoreManager.ProductListReceivedEvent -= ProductListReceived;
            IosStoreManager.ProductListRequestFailedEvent -= ProductListFailed;
            IosStoreManager.PurchaseFailedEvent -= PurchaseFailed;
            IosStoreManager.PurchaseCancelledEvent -= PurchaseCanceled;
            IosStoreManager.PurchaseSuccessfulEvent -= PurchaseFinished;
            IosStoreManager.TransactionUpdatedEvent -= TransactionUpdated;
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            var handler = PurchaseUpdated;
            if(handler != null)
            {
                handler(state, productID);
            }
        }
        #endif
    }
}
