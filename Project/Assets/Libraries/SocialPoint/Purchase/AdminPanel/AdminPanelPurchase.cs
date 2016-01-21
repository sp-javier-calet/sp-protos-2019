using UnityEngine;
using SocialPoint.AdminPanel;

namespace SocialPoint.Purchase
{
    public class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        SocialPointPurchaseStore _purchaseStore;

        public AdminPanelPurchase(SocialPointPurchaseStore purchaseStore)
        {
            _purchaseStore = purchaseStore;

            #if UNITY_EDITOR
            SetMockupProductsAndDelegate();
            #endif
            //Load products (IMPORTANT: Check that product IDs are set in PurchaseInstaller prefab)
            _purchaseStore.LoadProducts(_purchaseStore.StoreProductIds);
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            if(_purchaseStore != null)
            {
                adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("Purchase", this));
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            //TODO: Update GUI upon events (ProductsUpdated, PurchasesUpdated, etc)
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
