
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
        const int RESULT_USER_CANCELED = 1;

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
            Debug.Log("*** TEST LoadProducts...");
            if(!_isInitialized)
            {
                DebugLog("OpenIAB is not ready");
                return;
            }

            DebugLog("Mapping products on OpenIAB");
            /*foreach(string productId in productIds)
            {
                //OpenIAB.mapSku(productId, OpenIAB_Android.STORE_GOOGLE, productId);
            }*/

            DebugLog("Querying products");
            foreach(var item in productIds)
            {
                Debug.Log("*** TEST LoadProduct: " + item);
            }
            //OpenIAB.queryInventory(productIds);
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
                //OpenIAB.purchaseProduct(productId, string.Empty);
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

        //Right now this function should be coupled with a reload of the inventory (see AdminPanelPurchase for example)
        //TODO: Change to do the force in one go (as Mock and iOS stores)
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
            AndroidStoreManager.QueryInventorySucceededEvent += ProductListReceived;//QueryInventorySucceeded;
            AndroidStoreManager.QueryInventoryFailedEvent += QueryInventoryFailed;
            AndroidStoreManager.PurchaseSucceededEvent += PurchaseSucceeded;
            AndroidStoreManager.PurchaseFailedEvent += PurchaseFailed;
            AndroidStoreManager.ConsumePurchaseSucceededEvent += ConsumePurchaseSucceeded;
            AndroidStoreManager.ConsumePurchaseFailedEvent += ConsumePurchaseFailed;

            //OpenIAB.enableDebugLogging(true);
            /*Options options = new Options();
            options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
            options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
            options.checkInventory = false;
            options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
            options.prefferedStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
            options.availableStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
            options.storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT;*/


            DebugLog("setting options");
            //OpenIAB.init(options);
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
            UnityEngine.Debug.Log("*** TEST Received total products: " + products.Count);
            DebugLog("received total products: " + products.Count);
            try
            {
                foreach(AndroidStoreProduct product in products)
                {
                    Product parsedProduct = new Product(product.Sku, product.Title, float.Parse(product.PriceValue), product.CurrencyCode, product.Price);
                    UnityEngine.Debug.Log("*** TEST Parsed Product: " + parsedProduct.ToString());
                    DebugLog(parsedProduct.ToString());
                    _products.Add(parsedProduct);
                }
            }
            catch(Exception ex)
            {
                UnityEngine.Debug.Log("*** TEST Parsing went wrong");
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
            data.SetValue(Receipt.OriginalJsonKey, purchase.OriginalJson);
            data.SetValue(Receipt.StoreKey, "google_play");
            data.SetValue(Receipt.DataSignatureKey, purchase.Signature);
            UnityEngine.Debug.Log("*** TEST PurchaseSucceeded _validatePurchase: " + _validatePurchase);
            if(_validatePurchase != null)
            {
                Receipt receipt = new Receipt(data);
                _validatePurchase(receipt, (response) => {
                    //TODO: we have to consume the consumable items
                    UnityEngine.Debug.Log("*** TEST response from backend: " + response);
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        UnityEngine.Debug.Log("*** TEST Consuming product");
                        //OpenIAB.consumeProduct(purchase);
                        AndroidStoreBinding.FinishPendingTransaction(purchase.Sku);
                        PurchaseUpdated(PurchaseState.PurchaseFinished, receipt.ProductId);
                    }
                });
            }
        }

        private void PurchaseFailed(int errorCode, string error)
        {
            switch(errorCode)
            {
            case RESULT_USER_CANCELED:
                PurchaseUpdated(PurchaseState.PurchaseCanceled, _productId);
                break;
            default:
                PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
                break;
            }

            DebugLog(string.Format("Purchase failed : errorCode = {0}, error message = {1}",
                errorCode, error));
            DebugLog(errorCode.ToString());
        }

        private void BillingSupported()
        {
            _isInitialized = true;

            DebugLog("billingSupportedEvent");
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
            DebugLog(string.Format("Purchase Cancel : errorCode = {0}",
                error));
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
        }

        void UnregisterEvents()
        {
            AndroidStoreManager.BillingSupportedEvent -= BillingSupported;
            AndroidStoreManager.BillingNotSupportedEvent -= BillingNotSupported;
            AndroidStoreManager.QueryInventorySucceededEvent -= ProductListReceived;//QueryInventorySucceeded;
            AndroidStoreManager.QueryInventoryFailedEvent -= QueryInventoryFailed;
            AndroidStoreManager.PurchaseSucceededEvent -= PurchaseSucceeded;
            AndroidStoreManager.PurchaseFailedEvent -= PurchaseFailed;
            AndroidStoreManager.ConsumePurchaseSucceededEvent -= ConsumePurchaseSucceeded;
            AndroidStoreManager.ConsumePurchaseFailedEvent -= ConsumePurchaseFailed;
            //OpenIAB.unbindService();
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            PurchaseUpdated(state, productID);
        }
        #endif
    }
}
