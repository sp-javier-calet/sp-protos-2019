using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Attributes;

namespace SocialPoint.Purchase
{
    public class EmptyPurchaseStore : IPurchaseStore
    {
        public EmptyPurchaseStore()
        {
        }

        #region IPurchaseStore implementation

        public event ProductsUpdatedDelegate ProductsUpdated;
        public event PurchaseUpdatedDelegate PurchaseUpdated;

        protected virtual void OnProductsUpdated(LoadProductsState state, Error error)
        {
            var handler = ProductsUpdated;
            if(handler != null)
            {
                handler(state, error);
            }
        }

        protected virtual void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            var handler = PurchaseUpdated;
            if(handler != null)
            {
                handler(state, productId);
            }
        }

        public GetUserIdDelegate GetUserId
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public void LoadProducts(string[] productIds)
        {
            throw new NotImplementedException();
        }

        public bool Purchase(string productId)
        {
            throw new NotImplementedException();
        }

        public void ForceFinishPendingTransactions()
        {
            throw new NotImplementedException();
        }

        public bool HasProductsLoaded
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Product[] ProductList
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ValidatePurchaseDelegate ValidatePurchase
        {
            set
            {
                throw new NotImplementedException();
            }
        }

        public void PurchaseStateChanged(PurchaseState state, string productID)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable implementation

        virtual public void Dispose()
        {
            return;
        }

        #endregion
    }
}

