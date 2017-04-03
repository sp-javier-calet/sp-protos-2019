#if ADMIN_PANEL 

using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.ServerSync;

namespace SocialPoint.Purchase
{
    public sealed class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        IStoreProductSource _productSource;
        IGamePurchaseStore _purchaseStore;
        ICommandQueue _commandQueue;

        //Last known ids of products attempted to use
        string _lastKnownRequiredProducts;
        //Last known load state
        string _lastKnownLoadState;
        //Map each product ID to a last known purchase state message
        Dictionary<string, string> _lastKnownPurchaseState = new Dictionary<string, string>();

        //Flag to do purchase actions after some delay
        bool _purchaseWithDelay;

        //
        AdminPanelLayout _layout;

        public AdminPanelPurchase(IStoreProductSource productSource, IGamePurchaseStore purchaseStore, ICommandQueue commandQueue)
        {
            _productSource = productSource;
            _purchaseStore = purchaseStore;
            _purchaseStore.ProductsUpdated += OnProductsUpdated;
            _purchaseStore.PurchaseUpdated += OnPurchaseUpdated;
            _commandQueue = commandQueue;
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

            //TODO: Add options to activate an "always fail", "always success" response?

            //Load products
            layout.CreateLabel("Load Products");
            var productsInput = layout.CreateTextInput();
            AddGUIInfoLabel(layout, "Fill with comma separated IDs (ex: 1,2,3) or leave empty to load all");
            layout.CreateButton("Load", () => {
                string[] ids = string.IsNullOrEmpty(productsInput.text) ? null : productsInput.text.Split(',');
                LoadProducts(ids);
                layout.Refresh();
            });
            AddGUIInfoLabel(layout, "Latest required products: " + _lastKnownRequiredProducts);
            AddGUISeparation(layout);

            layout.CreateLabel("Purchase Options");
            //Use delay before purchasing? Can be used to test receiving events to refresh after closing the panel
            layout.CreateToggleButton("Purchase with delay?", _purchaseWithDelay, selected => {
                _purchaseWithDelay = selected;
                layout.Refresh();
            });
            //Force pending transactions
            layout.CreateButton("Finish Pending Transactions", _purchaseStore.ForceFinishPendingTransactions);
            //Force command queue flush
            layout.CreateConfirmButton("Flush Command Queue", () => {
                _commandQueue.Flush();
                _commandQueue.Send();
            });
            AddGUIInfoLabel(layout, "Flush after purchase validation if testing without a game to consume purchases");
            AddGUISeparation(layout);

            //In-Apps
            layout.CreateLabel("Products");
            if(_purchaseStore.HasProductsLoaded)
            {
                for(int i = 0, _purchaseStoreProductListLength = _purchaseStore.ProductList.Length; i < _purchaseStoreProductListLength; i++)
                {
                    Product product = _purchaseStore.ProductList[i];
                    AdminPanelLayout.VerticalLayout pLayout = layout.CreateVerticalLayout();
                    string id = product.Id;
                    //Caching id to avoid passing reference to lambda
                    //Purchase product button
                    pLayout.CreateButton(product.Locale, () => PurchaseProduct(id));
                    //Label with purchase state
                    if(_lastKnownPurchaseState.ContainsKey(id) && !string.IsNullOrEmpty(_lastKnownPurchaseState[id]))
                    {
                        var infoPanel = layout.CreatePanelLayout(product.Locale + " - Purchase Info", () => {
                            _lastKnownPurchaseState[id] = string.Empty;
                            layout.Refresh();
                        });
                        string purchaseMsg = _lastKnownPurchaseState[id];
                        AddGUIInfoLabel(infoPanel, purchaseMsg);
                    }
                }
            }
            else
            {
                AddGUIInfoLabel(layout, "< " + _lastKnownLoadState + " >");
            }
        }

        static void AddGUIInfoLabel(AdminPanelLayout layout, string label)
        {
            var infoLabel = layout.CreateLabel(label);
            infoLabel.fontSize /= 2;//Set info text as half the size of default label text
            infoLabel.fontStyle = FontStyle.Italic;
        }

        static void AddGUISeparation(AdminPanelLayout layout)
        {
            layout.CreateLabel("_________________");
            layout.CreateMargin(3);
        }

        void OnProductsUpdated(LoadProductsState state, Error error)
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
                _lastKnownLoadState = "Unknown State; " + state;
                break;
            }

            if(_layout != null)
            {
                _layout.Refresh();
            }
        }

        void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            _lastKnownPurchaseState[productId] = state.ToString();

            if(_layout != null)
            {
                _layout.Refresh();
            }
        }

        void LoadProducts(string[] ids = null)
        {
            #if UNITY_EDITOR
            //Mockup available products with latest data
            SetMockupProducts();
            #endif

            if(ids == null)
            {
                ids = _productSource.ProductIds;
            }

            _lastKnownRequiredProducts = MergeStrings(ids);
            _lastKnownLoadState = "Loading...";

            //Load products (IMPORTANT: Check that product IDs are set in game.json)
            _purchaseStore.LoadProducts(ids);
        }

        void PurchaseProduct(string productId)
        {
            if(_purchaseWithDelay)
            {
                ActionDelayer.Instance.FireActionWithDelay(() => _purchaseStore.Purchase(productId), 5.0f);
            }
            else
            {
                _purchaseStore.Purchase(productId);
            }
        }

        void SetMockupProducts()
        {
            //Create mockup product objects with mock store data
            string[] storeProductIds = _productSource.ProductIds;
            var mockProducts = new Product[storeProductIds.Length];
            for(int i = 0; i < mockProducts.Length; i++)
            {
                float price = (float)i + 0.99f;
                mockProducts[i] = new Product(
                    storeProductIds[i],
                    "Test Product " + (i + 1),
                    price,
                    "$",
                    price + "$"
                );
            }

            //Set products
            _purchaseStore.SetProductMockList(mockProducts);
        }

        static string MergeStrings(string[] ids)
        {
            string merged = string.Empty;
            for(int i = 0; i < ids.Length; i++)
            {
                merged += ids[i];
                if(i < ids.Length - 1)
                {
                    merged += ',';
                }
            }
            return merged;
        }
    }
}

#endif
