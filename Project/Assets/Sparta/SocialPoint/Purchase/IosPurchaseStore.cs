
using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Attributes;
using UnityEngine;

namespace SocialPoint.Purchase
{
    public class IosPurchaseStore
    #if UNITY_IOS
        : IPurchaseStore
    #endif
    {
        #if UNITY_IOS
        private List<Product> _products;
        private string _purchasingProduct;
        private List<Receipt> _pendingPurchases;

        #region IPurchaseStore implementationcategoryModel

        public event ProductsUpdatedDelegate ProductsUpdated = delegate {};

        public event PurchaseUpdatedDelegate PurchaseUpdated = delegate {};

        private ValidatePurchaseDelegate _validatePurchase;

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
                IosStoreBinding.PurchaseProduct(productId);
                _purchasingProduct = productId;
                PurchaseUpdated(PurchaseState.PurchaseStarted, productId);
                return true;
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

            IosStoreManager.autoConfirmTransactions = false;

            IosStoreManager.ProductListReceivedEvent += ProductListReceived;
            IosStoreManager.PurchaseFailedEvent += PurchaseFailed;
            IosStoreManager.PurchaseCancelledEvent += PurchaseCanceled;
            IosStoreManager.PurchaseSuccessfulEvent += PurchaseFinished;
            IosStoreManager.TransactionUpdatedEvent += TransactionUpdated;
            IosStoreManager.ProductPurchaseAwaitingConfirmationEvent += ProductPurchaseAwaitingConfirmation;
        }

        private void ProductListReceived(List<IosStoreProduct> products)
        {
            _products = new List<Product>();
            DebugLog("received total products: " + products.Count);
            try
            {
                foreach(IosStoreProduct product in products)
                {
                    Product parsedProduct = new Product(product.ProductIdentifier, product.Title, float.Parse(product.Price), product.CurrencySymbol, product.FormattedPrice);
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
            if(_pendingPurchases != null)
            {
                FinishPendingPurchases();
            }
        }

        private void FinishPendingPurchases()
        {
            if(_validatePurchase != null && _pendingPurchases.Count > 0)
            {
                Receipt receipt = _pendingPurchases[0];
                DebugLog("ProductPurchaseAwaitingConfirmation: " + receipt.ToString());
                UnityEngine.Debug.Log("*** TEST Validating Purchase. Caller: " + _validatePurchase);
                _validatePurchase(receipt, (response) => {
                    DebugLog("response given to IosPurchaseStore: " + response.ToString() + " for transaction: " + receipt.OrderId);
                    UnityEngine.Debug.Log("*** TEST Validation Backend Response: " + response.ToString());
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        UnityEngine.Debug.Log("*** TEST Finishing Transaction");
                        IosStoreBinding.FinishPendingTransaction(receipt.OrderId);
                        PurchaseUpdated(PurchaseState.PurchaseConsumed, receipt.ProductId);
                        _pendingPurchases.Remove(receipt);
                        FinishPendingPurchases();
                    }
                    //itunes api can only confirm a purchase(can't cancel) so we call nothing unless our backend says it's complete.
                });
                UnityEngine.Debug.Log("*** TEST Validation Sent");
            }
            else
            {
                DebugLog("All pending purchases finished");
            }
        }

        private void PurchaseFailed(string error)
        {
            DebugLog("PurchaseFailed " + error);
            UnityEngine.Debug.Log("*** TEST PurchaseFailed. Error: " + error);
            //_purchasingProduct may be uninitialized if the event comes when loading old (not consumed) transactions when the store is initialized
            if(!String.IsNullOrEmpty(_purchasingProduct))
            {
                PurchaseUpdated(PurchaseState.PurchaseFailed, _purchasingProduct);
            }
        }

        private void PurchaseCanceled(string error)
        {
            DebugLog("PurchaseCanceled " + error);
            UnityEngine.Debug.Log("*** TEST PurchaseCanceled. Error: " + error);
            //_purchasingProduct may be uninitialized if the event comes when loading old (not consumed) transactions when the store is initialized
            if(!String.IsNullOrEmpty(_purchasingProduct))
            {
                PurchaseUpdated(PurchaseState.PurchaseCanceled, _purchasingProduct);
            }
        }

        private void PurchaseFinished(IosStoreTransaction transaction)
        {
            DebugLog("Purchase has finished: " + transaction.TransactionIdentifier);
            UnityEngine.Debug.Log("*** TEST PurchaseFinished");
            PurchaseUpdated(PurchaseState.PurchaseFinished, transaction.ProductIdentifier);
        }

        private void ProductPurchaseAwaitingConfirmation(IosStoreTransaction transaction)
        {
            var data = new AttrDic();
            data.SetValue(Receipt.OrderIdKey, transaction.TransactionIdentifier);
            data.SetValue(Receipt.ProductIdKey, transaction.ProductIdentifier);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, transaction.Base64EncodedTransactionReceipt);
            data.SetValue(Receipt.StoreKey, "itunes");

            if(_pendingPurchases == null)
                _pendingPurchases = new List<Receipt>();

            _pendingPurchases.Add(new Receipt(data));

            if(_products != null && _products.Count > 0)
            {
                FinishPendingPurchases();
            }
        }

        private void TransactionUpdated(IosStoreTransaction transaction)
        {
            DebugLog("Transaction Updated: " + transaction.TransactionState);
        }

        void UnregisterEvents()
        {
            IosStoreManager.ProductListReceivedEvent -= ProductListReceived;
            IosStoreManager.PurchaseFailedEvent -= PurchaseFailed;
            IosStoreManager.PurchaseCancelledEvent -= PurchaseCanceled;
            IosStoreManager.PurchaseSuccessfulEvent -= PurchaseFinished;
            IosStoreManager.TransactionUpdatedEvent -= TransactionUpdated;
            IosStoreManager.ProductPurchaseAwaitingConfirmationEvent -= ProductPurchaseAwaitingConfirmation;
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
