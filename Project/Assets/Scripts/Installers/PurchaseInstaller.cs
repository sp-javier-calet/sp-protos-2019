using Zenject;
using System;
using UnityEngine;
using SocialPoint.AdminPanel;
using SocialPoint.Purchase;

public class PurchaseInstaller : MonoInstaller
{
    /// <summary>
    /// Fill these fields with real purchase data for this app
    /// </summary>
    [Serializable]
    public class StoreData
    {
        //Fill for Unity Editor testing
        [SerializeField]
        private string[] _mockupProductIds;

        [SerializeField]
        private string[] _androidProductIds;

        [SerializeField]
        private string[] _iosProductIds;

        public string[] ProductIds
        {
            get
            {
                string[] ids = null;
                #if UNITY_IOS && !UNITY_EDITOR
                ids = _iosProductIds;
                #elif UNITY_ANDROID && !UNITY_EDITOR
                ids = _androidProductIds;
                #elif UNITY_EDITOR
                ids = _mockupProductIds;
                #endif
                return ids;
            }
        }
    }
    //Instance to fill through editor
    public StoreData Data = new StoreData();

    public override void InstallBindings()
    {
        Container.BindInstance("purchase_store_product_ids", Data.ProductIds);

        Container.Rebind<SocialPointPurchaseStore>().ToSingle<PurchaseStore>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelPurchase>();
    }

}
