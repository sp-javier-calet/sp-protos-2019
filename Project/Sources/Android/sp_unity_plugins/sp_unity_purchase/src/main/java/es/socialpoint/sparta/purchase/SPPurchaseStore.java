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

    public static void PurchaseProduct(String productIdentifier)
    {
        //Possible debug ids:
        //android.test.purchased
        //android.test.canceled
        //android.test.refunded
        //android.test.item_unavailable
        if(_purchaseServices != null) {
            _purchaseServices.PurchaseProduct(productIdentifier);
        }
    }

    public void FinishPendingTransaction(String productIdentifier)
    {
        if(_purchaseServices != null) {
            _purchaseServices.FinishPendingTransaction(productIdentifier);
        }
    }
}
