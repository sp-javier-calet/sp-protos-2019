
using System;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.Attributes;
using OnePF;

namespace SocialPoint.Purchase
{
    public class AndroidPurchaseStore
        : IPurchaseStore
    {
        const int RESULT_USER_CANCELED = 1;

        private bool _isInitialized;
        private List<Product> _products;
        bool _autoCompletePurchases = false;
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

        public void LoadProducts(string[] productIds)
        {
            if(!_isInitialized)
            {
                DebugLog("OpenIAB is not ready");
                return;
            }

            DebugLog("Mapping products on OpenIAB");
            foreach(string productId in productIds)
            {
                OpenIAB.mapSku(productId, OpenIAB_Android.STORE_GOOGLE, productId);
            }

            DebugLog("Querying products");
            OpenIAB.queryInventory(productIds);
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
                OpenIAB.purchaseProduct(productId, string.Empty);
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
            _autoCompletePurchases = true;
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

            OpenIABEventManager.billingSupportedEvent += BillingSupported;
            OpenIABEventManager.billingNotSupportedEvent += BillingNotSupported;
            OpenIABEventManager.queryInventorySucceededEvent += QueryInventorySucceeded;
            OpenIABEventManager.queryInventoryFailedEvent += QueryInventoryFailed;
            OpenIABEventManager.purchaseSucceededEvent += PurchaseSucceeded;
            OpenIABEventManager.purchaseFailedEvent += PurchaseFailed;
            OpenIABEventManager.consumePurchaseSucceededEvent += consumePurchaseSucceeded;
            OpenIABEventManager.consumePurchaseFailedEvent += consumePurchaseFailed;

            OpenIAB.enableDebugLogging(true);
            Options options = new Options();
            options.checkInventoryTimeoutMs = Options.INVENTORY_CHECK_TIMEOUT_MS * 2;
            options.discoveryTimeoutMs = Options.DISCOVER_TIMEOUT_MS * 2;
            options.checkInventory = false;
            options.verifyMode = OptionsVerifyMode.VERIFY_SKIP;
            options.prefferedStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
            options.availableStoreNames = new string[] { OpenIAB_Android.STORE_GOOGLE };
            options.storeSearchStrategy = SearchStrategy.INSTALLER_THEN_BEST_FIT;


            DebugLog("setting options");
            OpenIAB.init(options);
        }

        [System.Diagnostics.Conditional("DEBUG_SPPURCHASE")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("AndroidPurchaseStore {0}", msg));
        }

        private void QueryInventorySucceeded(Inventory inventory)
        {
            //revise all pending purchases
            DebugLog(inventory.ToString());
            foreach(var item in inventory.GetAllPurchases())
            {
                if(_autoCompletePurchases)
                {
                    OpenIAB.consumeProduct(item);
                }
                else
                {
                    DebugLog("pending purchase: " + item);
                    PurchaseSucceeded(item);
                }
            }
            //This bool is set to false again to make the ForceFinishPendingTransactions a one time only action (check comments on function)
            _autoCompletePurchases = false;

            Debug.Log("received total products: " + inventory.GetAllAvailableSkus().Count);
            try
            {
                _products = new List<Product>();
                foreach(SkuDetails sk in inventory.GetAllAvailableSkus())
                {
                    Product parsedProduct = new Product(sk.Sku, sk.Title, float.Parse(sk.PriceValue), sk.CurrencyCode, sk.Price);
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

        private void QueryInventoryFailed(string error)
        {
            DebugLog("Query inventory failed");
            ProductsUpdated(LoadProductsState.Error, new Error(error));
        }

        private void PurchaseSucceeded(OnePF.Purchase purchase)
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
                Receipt receipt = new Receipt(data);
                _validatePurchase(receipt, (response) => {
                    //TODO: we have to consume the consumable items
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        OpenIAB.consumeProduct(purchase);
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

        public void consumePurchaseSucceeded(OnePF.Purchase obj)
        {
            PurchaseUpdated(PurchaseState.PurchaseConsumed, obj.Sku);
        }

        private void consumePurchaseFailed(string error)
        {
            DebugLog(string.Format("Purchase Cancel : errorCode = {0}",
                error));
            PurchaseUpdated(PurchaseState.PurchaseFailed, _productId);
        }

        void UnregisterEvents()
        {
            OpenIABEventManager.billingSupportedEvent -= BillingSupported;
            OpenIABEventManager.billingNotSupportedEvent -= BillingNotSupported;
            OpenIABEventManager.queryInventorySucceededEvent -= QueryInventorySucceeded;
            OpenIABEventManager.queryInventoryFailedEvent -= QueryInventoryFailed;
            OpenIABEventManager.purchaseSucceededEvent -= PurchaseSucceeded;
            OpenIABEventManager.purchaseFailedEvent -= PurchaseFailed;
            OpenIABEventManager.consumePurchaseSucceededEvent -= consumePurchaseSucceeded;
            OpenIABEventManager.consumePurchaseFailedEvent -= consumePurchaseFailed;
            OpenIAB.unbindService();
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            PurchaseUpdated(state, productID);
        }
    }
}
