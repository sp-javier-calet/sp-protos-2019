﻿using UnityEngine;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.ServerSync;

namespace SocialPoint.Purchase
{
    public class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        StoreModel _store;
        IGamePurchaseStore _purchaseStore;
        ICommandQueue _commandQueue;

        //Reference to layout
        AdminPanelLayout _layout;

        //Last known ids of products attempted to use
        string _lastKnownRequiredProducts;
        //Last known load state
        string _lastKnownLoadState;
        //Map each product ID to a last known purchase state message
        Dictionary<string, string> _lastKnownPurchaseState = new Dictionary<string, string>();

        //Flag to do purchase actions after some delay
        bool _purchaseWithDelay = false;

        public AdminPanelPurchase(StoreModel store, IGamePurchaseStore purchaseStore, ICommandQueue commandQueue)
        {
            _store = store;
            _purchaseStore = purchaseStore;
            _purchaseStore.ProductsUpdated += OnProductsUpdated;
            _purchaseStore.PurchaseUpdated += OnPurchaseUpdated;
            _commandQueue = commandQueue;

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

            //TODO: Add options to activate an "always fail", "always success" response?

            //Load products
            layout.CreateLabel("Load Products");
            var productsInput = layout.CreateTextInput();
            AddGUIInfoLabel(layout, "Fill with comma separated IDs (ex: 1,2,3) or leave empty to load all");
            layout.CreateButton("Load", () => {
                string[] ids = string.IsNullOrEmpty(productsInput.text) ? null : productsInput.text.Split(',');
                LoadProducts(ids);
            });
            AddGUIInfoLabel(layout, "Latest required products: " + _lastKnownRequiredProducts);
            AddGUISeparation(layout);

            layout.CreateLabel("Purchase Options");
            //Use delay before purchasing? Can be used to test receiving events to refresh after closing the panel
            layout.CreateToggleButton("Purchase with delay?", _purchaseWithDelay, (selected) => {
                _purchaseWithDelay = selected;
                RefreshPanel();
            });
            //Force pending transactions
            layout.CreateButton("Finish Pending Transactions", () => {
                _purchaseStore.ForceFinishPendingTransactions();
                #if UNITY_ANDROID && !UNITY_EDITOR
                //Reload... this is needed in Android. 
                //TODO: Check Mock and iOS versions to update Android version without needing to reload
                LoadProducts(null);
                #endif
            });
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
                foreach(Product product in _purchaseStore.ProductList)
                {
                    AdminPanelLayout.VerticalLayout pLayout = layout.CreateVerticalLayout();
                    string id = product.Id;//Caching id to avoid passing reference to lambda
                    //Purchase product button
                    pLayout.CreateButton(product.Locale, () => {
                        PurchaseProduct(id);
                    });
                    //Label with purchase state
                    if(_lastKnownPurchaseState.ContainsKey(id) && !string.IsNullOrEmpty(_lastKnownPurchaseState[id]))
                    {
                        var infoPanel = layout.CreatePanelLayout(product.Locale + " - Purchase Info", () => {
                            _lastKnownPurchaseState[id] = string.Empty;
                            RefreshPanel();
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

        private void AddGUIInfoLabel(AdminPanelLayout layout, string label)
        {
            var infoLabel = layout.CreateLabel(label);
            infoLabel.fontSize /= 2;//Set info text as half the size of default label text
            infoLabel.fontStyle = FontStyle.Italic;
        }

        private void AddGUISeparation(AdminPanelLayout layout)
        {
            layout.CreateLabel("_________________");
            layout.CreateMargin(3);
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
                _lastKnownLoadState = "Unknown State; " + state.ToString();
                break;
            }
            RefreshPanel();
        }

        private void OnPurchaseUpdated(PurchaseState state, string productId)
        {
            _lastKnownPurchaseState[productId] = state.ToString();
            RefreshPanel();
        }

        private void LoadProducts(string[] ids = null)
        {
            if(ids == null)
            {
                ids = _store.ProductIds;
            }

            _lastKnownRequiredProducts = MergeStrings(ids);
            _lastKnownLoadState = "Loading...";

            //Load products (IMPORTANT: Check that product IDs are set in game.json)
            _purchaseStore.LoadProducts(ids);
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
            _purchaseStore.RegisterPurchaseCompletedDelegate(OnMockPurchaseCompleted);
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

        private string MergeStrings(string[] ids)
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
