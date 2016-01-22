using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.AdminPanel;

namespace SocialPoint.Purchase
{
    public class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        SocialPointPurchaseStore _purchaseStore;

        //Map each product ID to a last known purchase state
        //TODO: Would it be possible to have more than one transaction in progress for a single product?
        Dictionary<string, PurchaseState> _lastKnownPurchaseState = new Dictionary<string, PurchaseState>();

        public AdminPanelPurchase(SocialPointPurchaseStore purchaseStore)
        {
            _purchaseStore = purchaseStore;
            _purchaseStore.ProductsUpdated += OnProductsUpdated;
            _purchaseStore.PurchaseUpdated += OnPurchaseUpdated;

            #if UNITY_EDITOR
            SetMockupProductsAndDelegate();
            #endif
            //Load products (IMPORTANT: Check that product IDs are set in PurchaseInstaller prefab)
            _purchaseStore.LoadProducts(_purchaseStore.StoreProductIds);
        }

        //IAdminPanelConfigurer implementation
        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_purchaseStore != null)
            {
                adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Purchase", this));
            }
        }

        //IAdminPanelGUI implementation
        public void OnCreateGUI(AdminPanelLayout layout)
        {
            //TODO: Add options to activate an "always fail", "always success" response?
            layout.CreateLabel("Products");//Title
            if(_purchaseStore.HasProductsLoaded)
            {
                foreach(Product product in _purchaseStore.ProductList)
                {
                    string id = product.Id;//Caching id to avoid passing reference to lambda
                    layout.CreateButton(product.Locale, 
                        () => {
                            OnPurchaseButtonClick(id);
                        });
                }
            }
            else
            {
                layout.CreateLabel("< Products Not Loaded >");
            }
        }

        private void OnPurchaseButtonClick(string productId)
        {
            _purchaseStore.Purchase(productId);
        }

        private void OnProductsUpdated(LoadProductsState state, Error error)
        {
            switch(state)
            {
            case LoadProductsState.Success:
                UnityEngine.Debug.Log("Products Loaded");
                break;
            case LoadProductsState.Error:
                UnityEngine.Debug.LogWarning("Products Load Error: " + error);
                break;
            default:
                UnityEngine.Debug.LogWarning("Unhandled Products Load State");
                break;
            }
        }

        private void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            _lastKnownPurchaseState[productId] = state;
        }

        private void SetMockupProductsAndDelegate()
        {
            //Create mockup product objects with mock store data
            Product[] mockProducts = new Product[_purchaseStore.StoreProductIds.Length];
            for(int i = 0; i < mockProducts.Length; i++)
            {
                float price = (float)i + 0.99f;
                mockProducts[i] = new Product(
                    _purchaseStore.StoreProductIds[i],
                    "Test Product " + (i + 1),
                    price,
                    "$",
                    price.ToString() + "$"
                );
            }

            //Set products
            _purchaseStore.SetProductMockList(mockProducts);
            //Set purchase delegate
            _purchaseStore.PurchaseCompleted = OnMockPurchaseCompleted;
        }

        PurchaseGameInfo OnMockPurchaseCompleted(Receipt receipt, PurchaseResponseType response)
        {
            //TODO: Return info depending on receipt.State and response type. Return null if not completed?
            UnityEngine.Debug.Log("Product Purchased: " + receipt.ProductId);
            PurchaseGameInfo purchaseInfo = new PurchaseGameInfo();
            purchaseInfo.OfferName = "Product " + receipt.ProductId;
            purchaseInfo.ResourceName = "Mock";
            purchaseInfo.ResourceAmount = 1;
            purchaseInfo.AdditionalData = null;
            return purchaseInfo;
        }
    }
}
