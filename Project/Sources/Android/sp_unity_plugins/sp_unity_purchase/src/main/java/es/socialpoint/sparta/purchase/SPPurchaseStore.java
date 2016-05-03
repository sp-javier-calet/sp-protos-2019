package es.socialpoint.sparta.purchase;

import es.socialpoint.sparta.purchase.SPPurchaseNativeServices;

/**
 * Created by abarrera on 02/05/16.
 */
public class SPPurchaseStore {

    //Native service manager
    private static SPPurchaseNativeServices _purchaseServices;

    public static void Init(String listenerObjectName)
    {
        _purchaseServices = SPPurchaseNativeServices.instance;
        if(_purchaseServices != null) {
            _purchaseServices.Init(listenerObjectName);
        }
    }

    public static void RequestProductData()
    {
        if(_purchaseServices != null) {
            _purchaseServices.LoadProducts();
        }
    }
}
