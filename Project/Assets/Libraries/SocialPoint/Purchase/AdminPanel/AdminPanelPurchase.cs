using SocialPoint.AdminPanel;

namespace SocialPoint.Purchase
{
    public class AdminPanelPurchase : IAdminPanelConfigurer, IAdminPanelGUI
    {
        SocialPointPurchaseStore _purchaseStore;

        Product[] _mockProducts = { new Product("0", "Test Product A", 0.99f, "$", "0.99 $") };

        public AdminPanelPurchase(SocialPointPurchaseStore purchaseStore)
        {
            _purchaseStore = purchaseStore;

            _purchaseStore.SetProductMockList(_mockProducts);
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
            //Load button
            layout.CreateButton("Load Products", OnLoadButtonClick);
            //Products area
            layout.CreateLabel("Products");
            if(_purchaseStore.HasProductsLoaded)
            {
                foreach(Product product in _purchaseStore.ProductList)
                {
                    layout.CreateButton(product.Locale, 
                        () => {
                            OnPurchaseButtonClick(product.Id);
                        });
                }
            }
        }

        private void OnLoadButtonClick()
        {
            //TODO: How do we obtain valid IDs to load? pre-defined by each game in some files?
            string[] productIds = new string[_mockProducts.Length];
            for(int i = 0; i < _mockProducts.Length; i++)
            {
                productIds[i] = _mockProducts[i].Id;
            }

            _purchaseStore.LoadProducts(productIds);
        }

        private void OnPurchaseButtonClick(string productId)
        {
            _purchaseStore.Purchase(productId);
        }
    }
}
