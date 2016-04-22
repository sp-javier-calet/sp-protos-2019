using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using SocialPoint.Base;
using UnityEngine.Assertions;

namespace SocialPoint.Purchase
{
    public class PurchaseGameInfo
    {
        public string OfferName;
        public string ResourceName;
        public int ResourceAmount;
        public AttrDic AdditionalData;
    }

    public delegate PurchaseGameInfo PurchaseCompletedDelegate(Receipt receipt, PurchaseResponseType response);

    public interface IGamePurchaseStore
    {
        event ProductsUpdatedDelegate ProductsUpdated;
        event PurchaseUpdatedDelegate PurchaseUpdated;

        Product[] ProductList { get; }

        bool HasProductsLoaded { get; }

        void LoadProducts(string[] productIds);

        void SetProductMockList(IEnumerable<Product> productMockList);

        bool Purchase(string productId);

        void RegisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate);

        void UnregisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate);

        void ForceFinishPendingTransactions();
    }

    //TODO: Verify behaviour for desired empty store
    public class EmptyGamePurchaseStore : IGamePurchaseStore
    {
        Product[] _productList = new Product[0];
        bool _productsLoaded = false;
        PurchaseCompletedDelegate _purchaseCompleted;

        public event ProductsUpdatedDelegate ProductsUpdated;
        public event PurchaseUpdatedDelegate PurchaseUpdated;

        public Product[] ProductList { get { return _productList; } }

        public bool HasProductsLoaded { get { return _productsLoaded; } }

        public void LoadProducts(string[] productIds)
        {
            if(ProductsUpdated != null)
            {
                ProductsUpdated(LoadProductsState.Success);
            }
            _productsLoaded = true;
        }

        public void SetProductMockList(IEnumerable<Product> productMockList)
        {
            //TODO: Allow mock products for this class?
        }

        public bool Purchase(string productId)
        {
            if(PurchaseUpdated != null)
            {
                PurchaseUpdated(PurchaseState.PurchaseFailed, productId);
            }
            if(_purchaseCompleted != null)
            {
                _purchaseCompleted(new Receipt(), PurchaseResponseType.Complete);
            }
            return true;
        }

        /// <summary>
        /// Registers the purchase completed delegate.
        /// May throw an exception if another delegate is already registered.
        /// </summary>
        /// <param name="pDelegate">Delegate to register.</param>
        public void RegisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate)
        {
            if(_purchaseCompleted != null && _purchaseCompleted != pDelegate)
            {
                throw new Exception("Only one delegate allowed!");
            }
            _purchaseCompleted = pDelegate;
        }

        /// <summary>
        /// Check if the current registered delegate matches with the param and unregister it if true
        /// </summary>
        /// <param name="pDelegate">Delegate to unregister.</param>
        public void UnregisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate)
        {
            if(_purchaseCompleted == pDelegate)
            {
                _purchaseCompleted = null;
            }
        }

        public void ForceFinishPendingTransactions()
        {
        }
    }

    public class SocialPointPurchaseStore : IGamePurchaseStore
    {
        IPurchaseStore _purchaseStore = null;
        IHttpClient _httpClient;
        ICommandQueue _commandQueue;
        List<string> _purchasesInProcess;

        /// <summary>
        /// The purchase completed function that each game defines.
        /// - check receipt product
        /// - apply changes depending the response
        /// - create a sync command if response was Complete
        /// </summary>
        PurchaseCompletedDelegate _purchaseCompleted;

        public string Currency;

        public bool ProductListReceived { get; private set; }

        const string HttpParamOrderData = "order_data_base64";
        //used for android
        const string HttpParamPurchaseData = "purchaseData";
        const string HttpParamDataSignature = "dataSignature";
        const string AttrKeyStatus = "status";
        const string EventNameMonetizationTransactionStart = "monetization.transaction_start";

        enum BackendResponse
        {
            ORDER_INVALID = 480,
            ORDER_NOTSYNC = 264,
            ORDER_SYNCED = 265
        }

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        public delegate void RequestSetupDelegate(HttpRequest req, string Uri);

        /// <summary>
        /// Should be connected to the event tracker to track purchase events
        /// TrackEvent = EventTracker.TrackSystemEvent
        /// </summary>
        public TrackEventDelegate TrackEvent;

        /// <summary>
        /// The request setup.
        /// </summary>
        public RequestSetupDelegate RequestSetup;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Purchase.SocialPointPurchaseStore"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        /// <param name = "commandQueue"></param>
        public SocialPointPurchaseStore(IHttpClient httpClient, ICommandQueue commandQueue)
        {
            #if UNITY_IOS && !UNITY_EDITOR
            _purchaseStore = new IosPurchaseStore();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            _purchaseStore = new AndroidPurchaseStore();
            #elif UNITY_EDITOR
            _purchaseStore = new MockPurchaseStore();
            #endif

            _httpClient = httpClient;
            _commandQueue = commandQueue;
            _purchaseStore.ValidatePurchase = SocialPointValidatePurchase;
            _purchasesInProcess = new List<string>();
            ProductListReceived = false;
            RegisterEvents();
        }

        [System.Diagnostics.Conditional("DEBUG_SPPURCHASE")]
        void DebugLog(string msg)
        {
            DebugUtils.Log(string.Format("IosPurchaseStore {0}", msg));
        }

        /// <summary>
        /// SocialPoint validate purchase process.
        /// Connects with backend and to validate a purchase, then calls OnBackendResponse
        /// this will be automatically called by the store's implementations when inventory
        /// is updated and there are pending purchases
        /// </summary>
        /// <param name="receipt">Receipt.</param>
        /// <param name="response">callback defined by each store implementation (usually consumes product, finishes transaction)</param>
        void SocialPointValidatePurchase(Receipt receipt, ValidatePurchaseResponseDelegate response)
        {
            if(_purchaseStore is MockPurchaseStore)
            {
                DebugLog("no validation for mockup purchase");
                var purchaseGameInfo = _purchaseCompleted(receipt, PurchaseResponseType.Complete);
                TrackPurchaseStart(receipt, purchaseGameInfo);
                return;
            }

            DebugLog("validating purchase with backend");

            HttpRequest req = new HttpRequest();
            //get it from SocialPointLogin
            if(RequestSetup != null)
            {
                RequestSetup(req, UriPayment);
                DebugUtils.Log(req.Url.AbsoluteUri);
            }

            #if UNITY_IOS
            req.AddParam(HttpParamOrderData, receipt.OriginalJson);
            #elif UNITY_ANDROID
            var paramDic = new AttrDic();
            paramDic.Set(HttpParamPurchaseData, new AttrString(receipt.OriginalJson));
            paramDic.Set(HttpParamDataSignature, new AttrString(receipt.DataSignature));
            req.AddParam(HttpParamOrderData, new JsonAttrSerializer().SerializeString(paramDic));
            #endif
            _httpClient.Send(req, (_1) => OnBackendResponse(_1, response, receipt));
        }

        /// <summary>
        /// parses backend response of a purchase validation
        /// responses can be:
        ///     480 purchase not valid
        ///     264 purchase pending to sync
        ///     265 purchase already synced
        /// </summary>
        /// <param name="response">callback defined by each store implementation (usually consumes product, finishes transaction)</param>
        void OnBackendResponse(HttpResponse resp, ValidatePurchaseResponseDelegate response, Receipt receipt)
        {
            //parse response from backend and call response with the final decission
            //JsonAttrParser parser = new JsonAttrParser();
            //AttrDic Data = parser.Parse(resp.Body).AsDic;
            DebugLog("parsing backend response");

            //switch(Data[AttrKeyStatus].AsValue.ToInt())
            switch(resp.StatusCode)
            {
            case (int)BackendResponse.ORDER_INVALID:
                //warn client
                _purchaseCompleted(receipt, PurchaseResponseType.Error);
                //consume purchase
                response(PurchaseResponseType.Error);
                break;
            case 200:
            case (int)BackendResponse.ORDER_NOTSYNC:
                //notify the store about validation state
                _purchaseStore.PurchaseStateChanged(PurchaseState.ValidateSuccess, receipt.ProductId);

                //response will be called on the response of the purchase cmd
                PurchaseSync(receipt, response);

                //client have to apply changes to de user_data
                var purchaseGameInfo = _purchaseCompleted(receipt, PurchaseResponseType.Complete);
                TrackPurchaseStart(receipt, purchaseGameInfo);
                //we send the packet with the purchaseSync, if there is no syncCmd we will add one with the cmdqueue event Sync
                _commandQueue.Send();
                break;
                
            case (int)BackendResponse.ORDER_SYNCED:
                //warn client
                _purchaseCompleted(receipt, PurchaseResponseType.Duplicated);
                //consume purchase
                response(PurchaseResponseType.Duplicated);
                break;

            default:
                response(PurchaseResponseType.Error);
                _purchaseCompleted(receipt, PurchaseResponseType.Error);
                break;
            }
        }

        void TrackPurchaseStart(Receipt receipt, PurchaseGameInfo info)
        {
            if(TrackEvent == null || info == null)
            {
                return;
            }
            var data = info.AdditionalData ?? new AttrDic();            
            var order = new AttrDic();
            data.Set("order", order);
            order.SetValue("transaction_id", receipt.OrderId);
            order.SetValue("product_id", receipt.ProductId);
            order.SetValue("payment_provider", receipt.Store);
            order.SetValue("offer", info.OfferName);
            order.SetValue("resource_type", info.ResourceName);
            order.SetValue("resource_amount", info.ResourceAmount);

            if(ProductList.Length > 0)
            {
                var products = new List<Product>(ProductList);
                var product = products.Find(x => x.Id == receipt.ProductId);
                if(product.Id != default(Product).Id)
                {
                    order.SetValue("amount_gross", product.Price);
                }
            }
            TrackEvent(EventNameMonetizationTransactionStart, data);
        }

        private void PurchaseSync(Receipt receipt, ValidatePurchaseResponseDelegate response)
        {
            var purchaseCmd = new PurchaseCommand(receipt.OrderId, receipt.Store);
            _commandQueue.Add(purchaseCmd, (data, err) => {
                if(Error.IsNullOrEmpty(err))
                {
                    DebugUtils.Log("calling ValidatePurchaseResponseDelegate"); 
                    response(PurchaseResponseType.Complete);
                }
                else
                {
                    DebugUtils.Log("command sync had an error"); 
                    //warn about an error
                }
            });
        }

        /// <summary>
        /// Gets the URI payment.
        /// </summary>
        /// <value>The URI payment.</value>
        public string UriPayment
        {
            get
            {
                #if UNITY_IOS && !UNITY_EDITOR
                return "purchase/itunes";
                #elif UNITY_ANDROID && !UNITY_EDITOR
                return "purchase/google_play";
                #elif UNITY_EDITOR
                return "purchase/unity";
                #else
                return "purchase/unknown";
                #endif
            }
        }

        /// <summary>
        /// Gets the product list.
        /// </summary>
        /// <value>The product list.</value>
        public Product[] ProductList
        {
            get { return _purchaseStore.ProductList; }
        }

        /// <summary>
        /// Gets if has products loaded.
        /// </summary>
        /// <value>The product list.</value>
        public bool HasProductsLoaded
        {
            get { return _purchaseStore.HasProductsLoaded; }
        }

        /// <summary>
        /// Loads the products.
        /// </summary>
        /// <param name="productIds">Product identifiers.</param>
        public void LoadProducts(string[] productIds)
        {
            _purchaseStore.LoadProducts(productIds);
        }

        /// <summary>
        /// Occurs when products updated.
        /// </summary>
        public event ProductsUpdatedDelegate ProductsUpdated
        {
            add
            {
                _purchaseStore.ProductsUpdated += value;
            }
            remove
            {
                _purchaseStore.ProductsUpdated -= value;
            }
        }

        /// <summary>
        /// Occurs when purchase is updated.
        /// </summary>
        public event PurchaseUpdatedDelegate PurchaseUpdated
        {
            add
            {
                _purchaseStore.PurchaseUpdated += value;
            }
            remove
            {
                _purchaseStore.PurchaseUpdated -= value;
            }
        }

        /// <summary>
        /// Purchase the specified productId.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        public bool Purchase(string productId)
        {
            //A delegate must exist before doing any attempt
            Assert.IsNotNull(_purchaseCompleted, "A PurchaseCompletedDelegate must be registered to handle purchase responses");

            UnityEngine.Debug.Log("Purchase: " + _purchasesInProcess.Contains(productId));
            if(_purchasesInProcess.Contains(productId))
            {
                //FIXME tech add purchasestate purchasealreadyinprocess
                _purchaseStore.PurchaseStateChanged(PurchaseState.AlreadyBeingPurchased, productId);
                return false;
            }
            _purchasesInProcess.Add(productId);
            return _purchaseStore.Purchase(productId);
        }

        /// <summary>
        /// Registers the purchase completed delegate.
        /// May throw an exception if another delegate is already registered.
        /// </summary>
        /// <param name="pDelegate">Delegate to register.</param>
        public void RegisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate)
        {
            if(_purchaseCompleted != null && _purchaseCompleted != pDelegate)
            {
                throw new Exception("Only one delegate allowed!");
            }
            _purchaseCompleted = pDelegate;
        }

        /// <summary>
        /// Check if the current registered delegate matches with the param and unregister it if true
        /// </summary>
        /// <param name="pDelegate">Delegate to unregister.</param>
        public void UnregisterPurchaseCompletedDelegate(PurchaseCompletedDelegate pDelegate)
        {
            if(_purchaseCompleted == pDelegate)
            {
                _purchaseCompleted = null;
            }
        }

        /// <summary>
        /// Autocompletes the pending purchases.
        /// </summary>
        public void ForceFinishPendingTransactions()
        {
            _purchaseStore.ForceFinishPendingTransactions();
        }

        public virtual void Dispose()
        {
            UnregisterEvents();
            _commandQueue = null;
            _purchaseStore.Dispose();
        }

        void RegisterEvents()
        {
            ProductsUpdated += OnCheckProducts;
            PurchaseUpdated += OnPurchaseUpdated;
        }

        void UnregisterEvents()
        {
            ProductsUpdated -= OnCheckProducts;
            PurchaseUpdated -= OnPurchaseUpdated;
        }

        void OnCheckProducts(LoadProductsState state, Error error)
        {
            if(state == LoadProductsState.Success)
            {
                if(ProductList.Length > 0)
                {
                    Currency = ProductList[0].Currency;
                }

                ProductListReceived = true;
            }
        }

        void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            switch(state)
            {
            case PurchaseState.PurchaseCanceled:
            case PurchaseState.PurchaseFailed:
            case PurchaseState.PurchaseConsumed:
                UnityEngine.Debug.Log("OnPurchaseUpdated: " + state + " " + productId);
                _purchasesInProcess.Remove(productId);
                break;
            }
        }

        public void SetProductMockList(IEnumerable<Product> productMockList)
        {
            if(_purchaseStore is MockPurchaseStore)
            {
                (_purchaseStore as MockPurchaseStore).SetProductMockList(productMockList);
            }
        }
    }
}
