#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Login;
using UnityEngine;
#endif

namespace SocialPoint.Purchase
{
    public sealed class AndroidPurchaseStore
    #if UNITY_ANDROID
        : IPurchaseStore
    #endif
    {
        #if UNITY_ANDROID
        bool _isInitialized;
        List<Product> _products;
        string _productId = string.Empty;
        AndroidStoreManager _storeManager;

        #region IPurchaseStore implementation

        public event ProductsUpdatedDelegate ProductsUpdated;

        public event PurchaseUpdatedDelegate PurchaseUpdated;

        public ILoginData LoginData { get; set; }

        ValidatePurchaseDelegate _validatePurchase;

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

        public void Setup(AttrDic settings)
        {
            PlatformPuchaseSettings.SetBoolSetting(settings, 
                PlatformPuchaseSettings.AndroidUseDetailedLogKey, 
                AndroidStoreBinding.EnableHighDetailLogs);
        }

        public void LoadProducts(string[] productIds)
        {
            if(!_isInitialized)
            {
                DebugLog("Purchase plugin is not ready");
                return;
            }

            DebugLog("Querying products");
            AndroidStoreBinding.RequestProductData(productIds);
        }

        public bool Purchase(string productId)
        {
            _productId = productId;
            if(_products == null)
            {
                DebugLog("there are no products, load them first");
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                return false;
            }
            DebugLog("buying product: " + productId);
            if(_products.Exists(p => p.Id == productId))
            {
                AndroidStoreBinding.PurchaseProduct(productId);
                PurchaseUpdated(PurchaseState.PurchaseStarted, productId);
                return true;
            }
            DebugLog("product doesn't exist: " + productId);
            PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
            return false;
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
                return (_products != null) ? _products.ToArray() : null;
            }
        }

        public void ForceFinishPendingTransactions()
        {
            AndroidStoreBinding.ForceFinishPendingTransactions();
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            UnregisterEvents();
        }

        #endregion

        public AndroidPurchaseStore(NativeCallsHandler handler)
        {
            if(Application.platform != RuntimePlatform.Android)
            {
                throw new NotImplementedException("AndroidPurchaseStore only works on Android");
            }

            _storeManager = new AndroidStoreManager(handler);
            _storeManager.BillingSupportedEvent += BillingSupported;
            _storeManager.BillingNotSupportedEvent += BillingNotSupported;
            _storeManager.QueryInventorySucceededEvent += ProductListReceived;
            _storeManager.QueryInventoryFailedEvent += QueryInventoryFailed;
            _storeManager.PurchaseSucceededEvent += PurchaseSucceeded;
            _storeManager.PurchaseFailedEvent += PurchaseFailed;
            _storeManager.PurchaseCancelledEvent += PurchaseCancelled;
            _storeManager.ConsumePurchaseSucceededEvent += ConsumePurchaseSucceeded;
            _storeManager.ConsumePurchaseFailedEvent += ConsumePurchaseFailed;
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugPurchasesFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("AndroidPurchaseStore {0}", msg));
        }

        void QueryInventoryFailed(Error error)
        {
            DebugLog("Query inventory failed");
            ProductsUpdated(LoadProductsState.Error, error);
        }

        void ProductListReceived(List<AndroidStoreProduct> products)
        {
            _products = new List<Product>();
            DebugLog("received total products: " + products.Count);
            try
            {
                for(int i = 0, productsCount = products.Count; i < productsCount; i++)
                {
                    AndroidStoreProduct product = products[i];
                    var parsedProduct = new Product(product.Sku, product.Title, float.Parse(product.PriceValue), product.CurrencyCode, product.Price);
                    DebugLog(parsedProduct.ToString());
                    _products.Add(parsedProduct);
                }
            }
            catch(Exception ex)
            {
                DebugLog("parsing went wrong");
                ProductsUpdated(LoadProductsState.Error, new Error(ex.Message));
            }
            DebugLog("all products parsed");
            ProductsUpdated(LoadProductsState.Success, null);
        }

        void PurchaseSucceeded(AndroidStoreTransaction purchase)
        {
            var data = new AttrDic();
            data.SetValue(Receipt.OrderIdKey, purchase.OrderId);
            data.SetValue(Receipt.ProductIdKey, purchase.Sku);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, purchase.OriginalJson);
            data.SetValue(Receipt.StoreKey, "google_play");
            data.SetValue(Receipt.DataSignatureKey, purchase.Signature);
            if(_validatePurchase != null)
            {
                var receipt = new Receipt(data);
                _validatePurchase(receipt, response => {
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        AndroidStoreBinding.FinishPendingTransaction(purchase.Sku);
                        PurchaseUpdated(PurchaseState.PurchaseFinished, receipt.ProductId);
                    }
                });
            }
        }

        void PurchaseFailed(Error error)
        {
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
            DebugLog(string.Format("Purchase failed : error message = {0}", error));
        }

        void PurchaseCancelled(Error error)
        {
            PurchaseUpdated(PurchaseState.PurchaseCanceled, _productId);
            DebugLog(string.Format("Purchase cancelled : error message = {0}", error));
        }

        void BillingSupported()
        {
            _isInitialized = true;
            DebugLog("BillingSupportedEvent");
        }

        void BillingNotSupported(Error error)
        {
            DebugLog("BillingNotSupportedEvent" + error);
        }

        public void ConsumePurchaseSucceeded(AndroidStoreTransaction purchase)
        {
            PurchaseUpdated(PurchaseState.PurchaseConsumed, purchase.Sku);
        }

        void ConsumePurchaseFailed(Error error)
        {
            DebugLog(string.Format("Purchase Cancel : errorCode = {0}", error));
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
        }

        void UnregisterEvents()
        {
            _storeManager.BillingSupportedEvent -= BillingSupported;
            _storeManager.BillingNotSupportedEvent -= BillingNotSupported;
            _storeManager.QueryInventorySucceededEvent -= ProductListReceived;
            _storeManager.QueryInventoryFailedEvent -= QueryInventoryFailed;
            _storeManager.PurchaseSucceededEvent -= PurchaseSucceeded;
            _storeManager.PurchaseFailedEvent -= PurchaseFailed;
            _storeManager.PurchaseCancelledEvent -= PurchaseCancelled;
            _storeManager.ConsumePurchaseSucceededEvent -= ConsumePurchaseSucceeded;
            _storeManager.ConsumePurchaseFailedEvent -= ConsumePurchaseFailed;
            AndroidStoreBinding.Unbind();
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            PurchaseUpdated(state, productID);
        }
        #endif
    }
}
