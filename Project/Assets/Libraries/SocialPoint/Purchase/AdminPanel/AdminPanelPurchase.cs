using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;

namespace SocialPoint.Purchase
{
    public class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        StoreModel _store;
        IGamePurchaseStore _purchaseStore;

        //Reference to layout
        AdminPanelLayout _layout;

        //Last known load state
        string _lastKnownLoadState;
        //Map each product ID to a last known purchase state message
        Dictionary<string, string> _lastKnownPurchaseState = new Dictionary<string, string>();

        //Flag to do purchase actions after some delay
        bool _purchaseWithDelay = false;

        public AdminPanelPurchase(StoreModel store, IGamePurchaseStore purchaseStore)
        {
            _store = store;
            _purchaseStore = purchaseStore;
            _purchaseStore.ProductsUpdated += OnProductsUpdated;
            _purchaseStore.PurchaseUpdated += OnPurchaseUpdated;

            #if UNITY_EDITOR
            SetMockupProductsAndDelegate();
            #endif
            LoadProducts();
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
            _layout = layout;
            int separationMargin = 3;

            //TODO: Add options to activate an "always fail", "always success" response?

            //Load products
            layout.CreateButton("Load Products", () => {
                LoadProducts();
            });
            layout.CreateMargin(separationMargin);

            //Use delay before purchasing? Can be used to test receiving events to refresh after closing the panel
            layout.CreateButton("Using Delay: " + _purchaseWithDelay, () => {
                _purchaseWithDelay = !_purchaseWithDelay;
                RefreshPanel();
            });
            layout.CreateMargin(separationMargin);

            //In-Apps
            layout.CreateLabel("Products");//Title
            if(_purchaseStore.HasProductsLoaded)
            {
                foreach(Product product in _purchaseStore.ProductList)
                {
                    string id = product.Id;//Caching id to avoid passing reference to lambda
                    //Purchase product button
                    layout.CreateButton(product.Locale, 
                        () => {
                            PurchaseProduct(id);
                        });
                    //Label with purchase state
                    if(_lastKnownPurchaseState.ContainsKey(id))
                    {
                        layout.CreateLabel(_lastKnownPurchaseState[id]);
                    }
                }
            }
            else
            {
                layout.CreateLabel(_lastKnownLoadState);
            }
        }

        private void OnProductsUpdated(LoadProductsState state, Error error)
        {
            switch(state)
            {
            case LoadProductsState.Success:
                _lastKnownLoadState = "Loaded";
                break;
            case LoadProductsState.Error:
                _lastKnownLoadState = "Load Error: " + error;
                break;
            default:
                _lastKnownLoadState = "Unknown State";
                break;
            }
            RefreshPanel();
        }

        private void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            _lastKnownPurchaseState[productId] = state.ToString();
            RefreshPanel();
        }

        private void LoadProducts()
        {
            _lastKnownLoadState = "Loading...";
            //Load products (IMPORTANT: Check that product IDs are set in game.json)
            _purchaseStore.LoadProducts(_store.ProductIds);
            RefreshPanel();
        }

        private void PurchaseProduct(string productId)
        {
            if(_purchaseWithDelay)
            {
                ActionDelayer.Instance.FireActionWithDelay(() => {
                    _purchaseStore.Purchase(productId);
                }, 5.0f);
            }
            else
            {
                _purchaseStore.Purchase(productId);
            }
        }

        private void RefreshPanel()
        {
            if(_layout != null && _layout.IsActiveInHierarchy)
            {
                _layout.Refresh();
            }
            else
            {
                _layout = null;//Clear previous reference
            }
        }

        private void SetMockupProductsAndDelegate()
        {
            //Create mockup product objects with mock store data
            string[] storeProductIds = _store.ProductIds;
            Product[] mockProducts = new Product[storeProductIds.Length];
            for(int i = 0; i < mockProducts.Length; i++)
            {
                float price = (float)i + 0.99f;
                mockProducts[i] = new Product(
                    storeProductIds[i],
                    "Test Product " + (i + 1),
                    price,
                    "$",
                    price.ToString() + "$"
                );
            }

            //Set products
            _purchaseStore.SetProductMockList(mockProducts);
            //Set purchase delegate
            _purchaseStore.PurchaseCompleted += OnMockPurchaseCompleted;
        }

        private PurchaseGameInfo OnMockPurchaseCompleted(Receipt receipt, PurchaseResponseType response)
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
