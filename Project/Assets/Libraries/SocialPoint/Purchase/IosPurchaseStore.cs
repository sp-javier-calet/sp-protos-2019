
using System;
using System.Collections.Generic;
using SocialPoint.Utils;
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

        public void LoadProducts (string[] productIds)
        {
            SocialPoint.Base.Debug.Log("requesting products");
            StoreKitBinding.requestProductData(productIds);
        }

        public bool Purchase (string productId)
        {
            if(_products == null)
            {
                //SocialPoint.Base.Debug.Log ("there are no products, load them first");
                PurchaseUpdated(PurchaseState.PurchaseFailed,productId);
                return false;
            }
            //SocialPoint.Base.Debug.Log ("buying product: " + productId);
            if(_products.Exists(p => p.Id == productId))
            {
                StoreKitBinding.purchaseProduct(productId, 1 );
                _purchasingProduct = productId;
                PurchaseUpdated(PurchaseState.PurchaseStarted,productId);
                return true;
            }
            else
            {
                //SocialPoint.Base.Debug.Log ("product doesn't exist: " + productId);
                PurchaseUpdated(PurchaseState.PurchaseFailed,productId);
                return false;
            }
        }

        public void ForceFinishPendingTransactions()
        {
            //SocialPoint.Base.Debug.Log ("ForceFinishPendingTransactions: ");
            StoreKitBinding.forceFinishPendingTransactions();
        }


        public Product[] ProductList{
            get
            {
                return _products.ToArray();
            }
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
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

        private void ProductListReceived (List<StoreKitProduct> products)
        {
            _products = new List<Product> ();
            //SocialPoint.Base.Debug.Log ("received total products: " + products.Count);
            try
            {
                foreach(StoreKitProduct product in products)
                {
                    Product parsedProduct = new Product(product.productIdentifier, product.title, float.Parse(product.price), product.currencySymbol);
                    SocialPoint.Base.Debug.Log (product.ToString());
                    _products.Add(parsedProduct);
                }

            }
            catch (Exception ex)
            {
                //SocialPoint.Base.Debug.Log ("parsing went wrong");
                ProductsUpdated (LoadProductsState.Error,new Error(ex.Message));
            }

            _products.Sort((Product p1, Product p2) => p1.Price.CompareTo(p2.Price));
            SocialPoint.Base.Debug.Log ("products sorted");
            ProductsUpdated (LoadProductsState.Success);
        }

        private void PurchaseFailed(string error)
        {
            //SocialPoint.Base.Debug.Log ("PurchaseFailed " + error);
            PurchaseUpdated (PurchaseState.PurchaseFailed, _purchasingProduct);
        }

        private void PurchaseCanceled(string error)
        {
            //SocialPoint.Base.Debug.Log ("PurchaseCanceled " +error);
            PurchaseUpdated (PurchaseState.PurchaseCanceled, _purchasingProduct);
        }

        private void PurchaseFinished(StoreKitTransaction transaction)
        {
            SocialPoint.Base.Debug.Log(transaction);
            PurchaseUpdated (PurchaseState.PurchaseFinished, transaction.productIdentifier);
            //FIXME Tech revise if needed
            /*
            if(_purchasingProduct == transaction.productIdentifier)//transaction finished, everything went ok
            {
                //SocialPoint.Base.Debug.Log ("Purchase has finished: " + transaction.transactionIdentifier);
                PurchaseUpdated (PurchaseState.PurchaseFinished, transaction.productIdentifier);
            }
            else
            {
                //different id's, something wrong happened
            }*/
        }

        private void ProductPurchaseAwaitingConfirmation(StoreKitTransaction transaction)
        {
            string id = transaction.transactionIdentifier;
            var data = new AttrDic ();
            data.SetValue(Receipt.OrderIdKey, transaction.transactionIdentifier);
            data.SetValue(Receipt.ProductIdKey, transaction.productIdentifier);
            data.SetValue(Receipt.PurchaseStateKey, (int)PurchaseState.ValidateSuccess);
            data.SetValue(Receipt.OriginalJsonKey, transaction.base64EncodedTransactionReceipt);
            data.SetValue(Receipt.StoreKey, "itunes");
            if(_validatePurchase != null)
            {
                Receipt receipt = new Receipt (data);
                SocialPoint.Base.Debug.Log ("ProductPurchaseAwaitingConfirmation: " + receipt.ToString());
                _validatePurchase(receipt, (response) => {
                    SocialPoint.Base.Debug.Log ("response given to IosPurchaseStore: " + response.ToString() + " for transaction: " + id);
                    if(response == PurchaseResponseType.Complete || response == PurchaseResponseType.Duplicated)
                    {
                        StoreKitBinding.finishPendingTransaction(id);
                        PurchaseUpdated (PurchaseState.PurchaseConsumed, transaction.productIdentifier);
                    }
                    //itunes api can only confirm a purchase(can't cancel) so we call nothing unless our backend says it's complete.
                });
            }

        }

        private void TransactionUpdated(StoreKitTransaction transaction)
        {
            SocialPoint.Base.Debug.Log ("Transaction Updated: " + transaction.transactionState);
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
