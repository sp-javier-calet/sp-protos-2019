
using System;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class AndroidPurchaseStore
    #if UNITY_ANDROID
        : IPurchaseStore
    #endif
    {
        #if UNITY_ANDROID
        private bool _isInitialized;
        private List<Product> _products;
        string _productId = string.Empty;


        #region IPurchaseStore implementation

        
        public event ProductsUpdatedDelegate ProductsUpdated;

        public event PurchaseUpdatedDelegate PurchaseUpdated;

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

        public GetUserIdDelegate GetUserId
        {
            set
            {
                //set if the user id is needed for this store
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
            else
            {
                DebugLog("product doesn't exist: " + productId);
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
                return false;
            }
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

        public void ForceFinishPendingTransactions()
        {
            AndroidStoreBinding.ForceFinishPendingTransactions();
        }

        

        #endregion

        
        

        #region IDisposable implementation

        
        virtual public void Dispose()
        {
            UnregisterEvents();
        }

        

        #endregion

        
        public AndroidPurchaseStore()
        {
            if(Application.platform != RuntimePlatform.Android)
            {
                throw new NotImplementedException("AndroidPurchaseStore only works on Android");
            }

            AndroidStoreManager.BillingSupportedEvent += BillingSupported;
            AndroidStoreManager.BillingNotSupportedEvent += BillingNotSupported;
            AndroidStoreManager.QueryInventorySucceededEvent += ProductListReceived;
            AndroidStoreManager.QueryInventoryFailedEvent += QueryInventoryFailed;
            AndroidStoreManager.PurchaseSucceededEvent += PurchaseSucceeded;
            AndroidStoreManager.PurchaseFailedEvent += PurchaseFailed;
            AndroidStoreManager.PurchaseCancelledEvent += PurchaseCancelled;
            AndroidStoreManager.ConsumePurchaseSucceededEvent += ConsumePurchaseSucceeded;
            AndroidStoreManager.ConsumePurchaseFailedEvent += ConsumePurchaseFailed;
        }

        [System.Diagnostics.Conditional("DEBUG_SPPURCHASE")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("AndroidPurchaseStore {0}", msg));
        }

        private void QueryInventoryFailed(string error)
        {
            DebugLog("Query inventory failed");
            ProductsUpdated(LoadProductsState.Error, new Error(error));
        }

        private void ProductListReceived(List<AndroidStoreProduct> products)
        {
            _products = new List<Product>();
            DebugLog("received total products: " + products.Count);
            try
            {
                foreach(AndroidStoreProduct product in products)
                {
                    Product parsedProduct = new Product(product.Sku, product.Title, float.Parse(product.PriceValue), product.CurrencyCode, product.Price);
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

        private void PurchaseSucceeded(AndroidStoreTransaction purchase)
        {
            var data = new AttrDic();
            data.SetValue(Receipt.OrderIdKey, purchase.OrderId);
            data.SetValue(Receipt.ProductIdKey, purchase.Sku);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, purchase.GetValidationData());//purchase.OriginalJson
            data.SetValue(Receipt.StoreKey, "google_play");
            data.SetValue(Receipt.DataSignatureKey, purchase.Signature);
            if(_validatePurchase != null)
            {
                Receipt receipt = new Receipt(data);
                _validatePurchase(receipt, (response) => {
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        AndroidStoreBinding.FinishPendingTransaction(purchase.Sku);
                        PurchaseUpdated(PurchaseState.PurchaseFinished, receipt.ProductId);
                    }
                });
            }
        }

        private void PurchaseFailed(string error)
        {
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
            DebugLog(string.Format("Purchase failed : error message = {0}", error));
        }

        private void PurchaseCancelled(string error)
        {
            PurchaseUpdated(PurchaseState.PurchaseCanceled, _productId);
            DebugLog(string.Format("Purchase cancelled : error message = {0}", error));
        }

        private void BillingSupported()
        {
            _isInitialized = true;
            DebugLog("BillingSupportedEvent");
        }

        private void BillingNotSupported(string error)
        {
            DebugLog("BillingNotSupportedEvent" + error.ToString());
        }

        public void ConsumePurchaseSucceeded(AndroidStoreTransaction purchase)
        {
            PurchaseUpdated(PurchaseState.PurchaseConsumed, purchase.Sku);
        }

        private void ConsumePurchaseFailed(string error)
        {
            DebugLog(string.Format("Purchase Cancel : errorCode = {0}", error));
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
        }

        void UnregisterEvents()
        {
            AndroidStoreManager.BillingSupportedEvent -= BillingSupported;
            AndroidStoreManager.BillingNotSupportedEvent -= BillingNotSupported;
            AndroidStoreManager.QueryInventorySucceededEvent -= ProductListReceived;
            AndroidStoreManager.QueryInventoryFailedEvent -= QueryInventoryFailed;
            AndroidStoreManager.PurchaseSucceededEvent -= PurchaseSucceeded;
            AndroidStoreManager.PurchaseFailedEvent -= PurchaseFailed;
            AndroidStoreManager.PurchaseCancelledEvent -= PurchaseCancelled;
            AndroidStoreManager.ConsumePurchaseSucceededEvent -= ConsumePurchaseSucceeded;
            AndroidStoreManager.ConsumePurchaseFailedEvent -= ConsumePurchaseFailed;
            AndroidStoreBinding.Unbind();
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            PurchaseUpdated(state, productID);
        }
        #endif
    }
}
