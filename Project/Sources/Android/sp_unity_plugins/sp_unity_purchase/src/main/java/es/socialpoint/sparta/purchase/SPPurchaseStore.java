package es.socialpoint.sparta.purchase;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

import es.socialpoint.sparta.purchase.SPPurchaseNativeServices;

/**
 * Created by abarrera on 02/05/16.
 */
public class SPPurchaseStore {

    //Native service manager
    private static SPPurchaseNativeServices _purchaseServices;

    public static void Init(String listenerObjectName)
    {
        _purchaseServices = new SPPurchaseNativeServices(listenerObjectName);
    }

    public static void RequestProductData()
    {
        if(_purchaseServices != null) {
            _purchaseServices.LoadProducts();
        }
    }
}
