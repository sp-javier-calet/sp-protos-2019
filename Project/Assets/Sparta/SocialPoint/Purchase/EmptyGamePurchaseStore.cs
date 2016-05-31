using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    //TODO: Verify behaviour for desired empty store
    public class EmptyGamePurchaseStore : IGamePurchaseStore
    {
        Product[] _productList = new Product[0];
        bool _productsLoaded = false;
        PurchaseCompletedDelegate _purchaseCompleted;

        public event ProductsUpdatedDelegate ProductsUpdated;
        public event PurchaseUpdatedDelegate PurchaseUpdated;

        public void Setup(AttrDic settings)
        {
            //Empty
        }

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
}