
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
            StoreKitBinding.requestProductData(productIds);
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
                StoreKitBinding.purchaseProduct(productId, 1);
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
            StoreKitBinding.forceFinishPendingTransactions();
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

            StoreKitManager.autoConfirmTransactions = false;

            StoreKitManager.productListReceivedEvent += ProductListReceived;
            StoreKitManager.purchaseFailedEvent += PurchaseFailed;
            StoreKitManager.purchaseCancelledEvent += PurchaseCanceled;
            StoreKitManager.purchaseSuccessfulEvent += PurchaseFinished;
            StoreKitManager.transactionUpdatedEvent += TransactionUpdated;
            StoreKitManager.productPurchaseAwaitingConfirmationEvent += ProductPurchaseAwaitingConfirmation;
        }

        private void ProductListReceived(List<StoreKitProduct> products)
        {
            _products = new List<Product>();
            DebugLog("received total products: " + products.Count);
            try
            {
                foreach(StoreKitProduct product in products)
                {
                    Product parsedProduct = new Product(product.productIdentifier, product.title, float.Parse(product.price), product.currencySymbol, product.formattedPrice);
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
                _validatePurchase(receipt, (response) => {
                    DebugLog("response given to IosPurchaseStore: " + response.ToString() + " for transaction: " + receipt.OrderId);
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        StoreKitBinding.finishPendingTransaction(receipt.OrderId);
                        PurchaseUpdated(PurchaseState.PurchaseConsumed, receipt.ProductId);
                        _pendingPurchases.Remove(receipt);
                        FinishPendingPurchases();
                    }
                    //itunes api can only confirm a purchase(can't cancel) so we call nothing unless our backend says it's complete.
                });
            }
            else
            {
                DebugLog("All pending purchases finished");
            }
        }

        private void PurchaseFailed(string error)
        {
            DebugLog("PurchaseFailed " + error);
            PurchaseUpdated(PurchaseState.PurchaseFailed, _purchasingProduct);
        }

        private void PurchaseCanceled(string error)
        {
            DebugLog("PurchaseCanceled " + error);
            PurchaseUpdated(PurchaseState.PurchaseCanceled, _purchasingProduct);
        }

        private void PurchaseFinished(StoreKitTransaction transaction)
        {
            DebugLog("Purchase has finished: " + transaction.transactionIdentifier);
            PurchaseUpdated(PurchaseState.PurchaseFinished, transaction.productIdentifier);
        }

        private void ProductPurchaseAwaitingConfirmation(StoreKitTransaction transaction)
        {
            var data = new AttrDic();
            data.SetValue(Receipt.OrderIdKey, transaction.transactionIdentifier);
            data.SetValue(Receipt.ProductIdKey, transaction.productIdentifier);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, transaction.base64EncodedTransactionReceipt);
            data.SetValue(Receipt.StoreKey, "itunes");

            if(_pendingPurchases == null)
                _pendingPurchases = new List<Receipt>();

            _pendingPurchases.Add(new Receipt(data));

            if(_products != null && _products.Count > 0)
            {
                FinishPendingPurchases();
            }
        }

        private void TransactionUpdated(StoreKitTransaction transaction)
        {
            DebugLog("Transaction Updated: " + transaction.transactionState);
        }

        void UnregisterEvents()
        {
            StoreKitManager.productListReceivedEvent -= ProductListReceived;
            StoreKitManager.purchaseFailedEvent -= PurchaseFailed;
            StoreKitManager.purchaseCancelledEvent -= PurchaseCanceled;
            StoreKitManager.purchaseSuccessfulEvent -= PurchaseFinished;
            StoreKitManager.transactionUpdatedEvent -= TransactionUpdated;
            StoreKitManager.productPurchaseAwaitingConfirmationEvent -= ProductPurchaseAwaitingConfirmation;
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
