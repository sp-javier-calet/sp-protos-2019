using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

namespace SocialPoint.Purchase
{
    public sealed class SocialPointPurchaseStore : IGamePurchaseStore
    {
        class ProductReadyPetition
        {
            public ProductReadyDelegate Callback { get; private set; }

            float _timeout;
            double _creationDateTimestamp;

            public ProductReadyPetition(ProductReadyDelegate pDelegate, float timeout)
            {
                Callback = pDelegate;
                _timeout = timeout;
                _creationDateTimestamp = GetTimestampDouble();
            }

            public bool IsExpired()
            {
                if(_timeout <= 0.0f)
                {
                    return false;//Expire only if a positive timeout was set
                }

                double deltaTime = GetTimestampDouble() - _creationDateTimestamp;
                return (deltaTime > _timeout);
            }

            static double GetTimestampDouble()
            {
                return TimeUtils.GetTimestampDouble(DateTime.Now);
            }
        }

        IPurchaseStore _purchaseStore;

        Dictionary<string, Action<PurchaseResponseType>> _purchasesInProcess;
        Dictionary<string, List<ProductReadyPetition>> _productReadyPetitions;

        public IHttpClient HttpClient { get; set; }
        public ICommandQueue CommandQueue { get; set; }

        /// <summary>
        /// The purchase completed function that each game defines.
        /// - check receipt product
        /// - apply changes depending the response
        /// - create a sync command if response was Complete
        /// </summary>
        PurchaseCompletedDelegate _purchaseCompleted;

        public string Currency { get; set; }

        public bool ProductListReceived { get; private set; }

        const string HttpParamOrderData = "order_data_base64";
        const string HttpParamOrderId = "order_id";
        const string HttpParamAppleReceiptEncoding = "apple_receipt_asn1_encoding";
        const string HttpValueDefaultAppleReceiptEncoding = "1";
        //used for android
        const string HttpParamPurchaseData = "purchaseData";
        const string HttpParamDataSignature = "dataSignature";
        const string AttrKeyStatus = "status";
        const string EventNameMonetizationTransactionStart = "monetization.transaction_start";


        // Payment step funnnels required params
        string _purchaseSessionUID;

        const string PurchaseFunnelStart = "1000";
        const string PurchaseFunnelBackendResponse = "1001";
        const string PurchaseFunnelEnd = "1002";

        const string PaymentStepEventName = "payment.step";
        const string PaymentFieldName = "payment";

        const string UIDFieldName = "uid";
        const string StepFieldName = "step";
        const string ResultFieldName = "result";
        const string ProductFieldName = "product";

        enum BackendResponse
        {
            ORDER_INVALID = 480,
            ORDER_NOTSYNC = 264,
            ORDER_SYNCED = 265
        }

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        /// <summary>
        /// Should be connected to the event tracker to track purchase events
        /// TrackEvent = EventTracker.TrackSystemEvent
        /// </summary>
        public TrackEventDelegate TrackEvent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.Purchase.SocialPointPurchaseStore"/> class.
        /// </summary>
        /// <param name="httpClient">Http client.</param>
        /// <param name = "commandQueue"></param>
        public SocialPointPurchaseStore(NativeCallsHandler handler)
        {
            #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
            _purchaseStore = new IosPurchaseStore(handler);
            #elif UNITY_ANDROID && !UNITY_EDITOR
            _purchaseStore = new AndroidPurchaseStore(handler);
            #else
            _purchaseStore = new MockPurchaseStore();
            #endif
            _purchaseStore.ValidatePurchase = SocialPointValidatePurchase;
            _purchasesInProcess = new Dictionary<string, Action<PurchaseResponseType>>();
            _productReadyPetitions = new Dictionary<string, List<ProductReadyPetition>>();
            ProductListReceived = false;
            RegisterEvents();
        }

        public void Setup(AttrDic settings)
        {
            _purchaseStore.Setup(settings);
        }

        public ILoginData LoginData
        {
            get
            {
                return _purchaseStore.LoginData;
            }
            set
            {
                _purchaseStore.LoginData = value;
            }
        }

        [System.Diagnostics.Conditional(DebugFlags.DebugPurchasesFlag)]
        void DebugLog(string msg)
        {
            Log.i(string.Format("SocialPointPurchaseStore {0}", msg));
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

            var req = new HttpRequest();
            //get it from SocialPointLogin
            if(LoginData != null)
            {
                LoginData.SetupHttpRequest(req, UriPayment);
                Log.i(req.Url.AbsoluteUri);
            }

            #if (UNITY_IOS || UNITY_TVOS)
            req.AddParam(HttpParamOrderData, receipt.OriginalJson);
            req.AddParam(HttpParamOrderId, receipt.OrderId);
            req.AddParam(HttpParamAppleReceiptEncoding, HttpValueDefaultAppleReceiptEncoding);
            #elif UNITY_ANDROID
            var paramDic = new AttrDic();
            paramDic.Set(HttpParamPurchaseData, new AttrString(receipt.OriginalJson));
            paramDic.Set(HttpParamDataSignature, new AttrString(receipt.DataSignature));
            req.AddParam(HttpParamOrderData, new JsonAttrSerializer().SerializeString(paramDic));
            #endif
            HttpClient.Send(req, _1 => OnBackendResponse(_1, response, receipt));
        }

        /// <summary>
        /// parses backend response of a purchase validation
        /// responses can be:
        ///     480 purchase not valid
        ///     264 purchase pending to sync
        ///     265 purchase already synced
        /// </summary>
        /// <param name = "resp"></param>
        /// <param name="response">callback defined by each store implementation (usually consumes product, finishes transaction)</param>
        /// <param name = "receipt"></param>
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
                CommandQueue.Send();
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
            TrackEvent(EventNameMonetizationTransactionStart, data, TrackerErrorDelegate);
        }

        void PurchaseSync(Receipt receipt, ValidatePurchaseResponseDelegate response)
        {
            var purchaseCmd = new PurchaseCommand(receipt.OrderId, receipt.Store);
            CommandQueue.Add(purchaseCmd, (data, err) => {
                if(Error.IsNullOrEmpty(err))
                {
                    Log.i("calling ValidatePurchaseResponseDelegate"); 
                    response(PurchaseResponseType.Complete);
                }
                else
                {
                    Log.i("command sync had an error"); 
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
                #if (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
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
        /// <param name = "finished"></param>
        public bool Purchase(string productId, Action<PurchaseResponseType> finished = null)
        {
            //A delegate must exist before doing any attempt
            DebugUtils.Assert(_purchaseCompleted != null, "A PurchaseCompletedDelegate must be registered to handle purchase responses");

            Log.i("Purchase: " + _purchasesInProcess.ContainsKey(productId));
            if(_purchasesInProcess.ContainsKey(productId))
            {
                _purchaseStore.PurchaseStateChanged(PurchaseState.AlreadyBeingPurchased, productId);
                if(finished != null)
                {
                    finished(PurchaseResponseType.Duplicated);
                }
                return false;
            }
            _purchasesInProcess.Add(productId, finished);
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

        public void RegisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate)
        {
            RegisterProductReadyDelegate(productId, pDelegate, 0.0f);
        }

        public void RegisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate, float timeout)
        {
            if(pDelegate == null)
            {
                return;
            }

            if(IsProductReady(productId))
            {
                pDelegate(productId);
                return;
            }

            var petition = new ProductReadyPetition(pDelegate, timeout);

            List<ProductReadyPetition> onProductReadyPetitions;
            if(!_productReadyPetitions.TryGetValue(productId, out onProductReadyPetitions))
            {
                onProductReadyPetitions = new List<ProductReadyPetition>();
                _productReadyPetitions.Add(productId, onProductReadyPetitions);
            }
            onProductReadyPetitions.Add(petition);
            return;
        }

        public void UnregisterProductReadyDelegate(string productId, ProductReadyDelegate pDelegate)
        {
            List<ProductReadyPetition> onProductReadyPetitions;
            if(_productReadyPetitions.TryGetValue(productId, out onProductReadyPetitions))
            {
                onProductReadyPetitions.RemoveAll(petition => petition.Callback == pDelegate);
            }
        }

        public void UnregisterProductReadyDelegate(ProductReadyDelegate pDelegate)
        {
            var itr = _productReadyPetitions.GetEnumerator();
            while(itr.MoveNext())
            {
                var entry = itr.Current;
                UnregisterProductReadyDelegate(entry.Key, pDelegate);
            }
            itr.Dispose();
        }

        void UpdateProductReadyPetitions(string productId)
        {
            List<ProductReadyPetition> onProductReadyPetitions;
            if(_productReadyPetitions.TryGetValue(productId, out onProductReadyPetitions))
            {
                for(int i = 0; i < onProductReadyPetitions.Count; ++i)
                {
                    ProductReadyPetition petition = onProductReadyPetitions[i];
                    if(petition.Callback != null && !petition.IsExpired())
                    {
                        petition.Callback(productId);
                    }
                }

                onProductReadyPetitions.Clear();
            }
        }

        bool IsProductReady(string productId)
        {
            return IsUpdatedFromStore(productId) && !IsPendingTransaction(productId);
        }

        bool IsUpdatedFromStore(string productId)
        {
            if(_purchaseStore.HasProductsLoaded)
            {
                Product[] currentProducts = ProductList;
                if(currentProducts != null)
                {
                    for(int i = 0; i < currentProducts.Length; ++i)
                    {
                        if(currentProducts[i].Id == productId)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        bool IsPendingTransaction(string productId)
        {
            return _purchasesInProcess.ContainsKey(productId);
        }

        public void Dispose()
        {
            UnregisterEvents();
            CommandQueue = null;
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
                Product[] loadedProducts = ProductList;
                if(loadedProducts != null)
                {
                    if(loadedProducts.Length > 0)
                    {
                        Currency = loadedProducts[0].Currency;
                    }

                    ProductListReceived = true;

                    for(int i = 0; i < loadedProducts.Length; ++i)
                    {
                        UpdateProductReadyPetitions(loadedProducts[i].Id);
                    }
                }
            }
        }

        void RemovePurchaseInProcess(string productId, PurchaseResponseType purchaseResponse)
        {
            Action<PurchaseResponseType> finished;
            if(_purchasesInProcess.TryGetValue(productId, out finished))
            {
                _purchasesInProcess.Remove(productId);
                if(finished != null)
                {
                    finished(purchaseResponse);
                }
            }
        }

        void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            TrackPurchaseUpdated(state, productId);

            switch(state)
            {
            case PurchaseState.PurchaseCanceled:
            case PurchaseState.PurchaseFailed:
            case PurchaseState.PurchaseConsumed:
                Log.i("OnPurchaseUpdated: " + state + " " + productId);
                var responseType = state == PurchaseState.PurchaseConsumed ? PurchaseResponseType.Complete : PurchaseResponseType.Error;
                RemovePurchaseInProcess(productId, responseType);
                UpdateProductReadyPetitions(productId);
                break;
            }
        }

        void TrackPurchaseUpdated(PurchaseState state, string productId)
        {
            switch(state)
            {
            case PurchaseState.PurchaseStarted:
                GeneratePurchaseSession();
                TrackPurchase(GetPurchaseSession(), PurchaseFunnelStart, state.ToString(), productId);
                break;

            case PurchaseState.PurchaseConsumed:
                TrackPurchase(GetPurchaseSession(), PurchaseFunnelEnd, state.ToString(), productId);
                ClearPurchaseSession();
                break;

            case PurchaseState.PurchaseCanceled:
                TrackPurchase(GetPurchaseSession(), PurchaseFunnelBackendResponse, state.ToString(), productId); // For Better readibility of the funnels.
                TrackPurchase(GetPurchaseSession(), PurchaseFunnelEnd, state.ToString(), productId);
                ClearPurchaseSession();
                break;

            case PurchaseState.ValidateFailed:
            case PurchaseState.ValidateFailedMissingNetwork:
            case PurchaseState.PurchaseFailed:
            case PurchaseState.AlreadyBeingPurchased:
                TrackPurchase(GetPurchaseSession(), PurchaseFunnelEnd, state.ToString(), productId);
                ClearPurchaseSession();
                break;
            }
        }

        void TrackPurchase(string uid, string step, string result, string productId)
        {
            var data = new AttrDic();

            var stepTrackData = CreatePaymentStepTrackData(uid, step, result, productId);

            data.Set(PaymentFieldName, stepTrackData);

            TrackEvent(PaymentStepEventName, data, TrackerErrorDelegate);
        }

        static AttrDic CreatePaymentStepTrackData(string uid, string step, string result, string productId)
        {
            var paymentStepData = new AttrDic();

            paymentStepData.SetValue(UIDFieldName, uid);
            paymentStepData.SetValue(StepFieldName, step);
            paymentStepData.SetValue(ResultFieldName, result);
            paymentStepData.SetValue(ProductFieldName, productId);

            return paymentStepData;
        }

        static void TrackerErrorDelegate(Error err)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                Log.i("[SocialPointPurchaseStore] TrackEvent Error - Code: " + err.Code + " - Message: " + err.Msg);
            }
        }

        void GeneratePurchaseSession()
        {
            _purchaseSessionUID = RandomUtils.GetUuid("N");
        }

        void ClearPurchaseSession()
        {
            _purchaseSessionUID = string.Empty;
        }

        string GetPurchaseSession()
        {
            return _purchaseSessionUID;
        }

        public void SetProductMockList(IEnumerable<Product> productMockList)
        {
            var mockPurchaseStore = _purchaseStore as MockPurchaseStore;
            if(mockPurchaseStore != null)
            {
                mockPurchaseStore.SetProductMockList(productMockList);
            }
        }
    }
}
