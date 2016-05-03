package es.socialpoint.sparta.purchase;

import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.ArrayList;

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

    public static void RequestProductData(String productIds)
    {
        if(_purchaseServices != null) {
            String[] ids = productIds.split(",");
            ArrayList<String> skus  = new ArrayList<String>();
            for (String p : ids) {
                skus.add(p);
            }
            _purchaseServices.LoadProducts(skus);
        }
    }
}
