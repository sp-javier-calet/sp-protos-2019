package es.socialpoint.sparta.purchase;

import java.util.ArrayList;

/**
 * Created by abarrera on 02/05/16.
 */
public class SPPurchaseStore {

    //Native service manager
    private static SPPurchaseNativeServices _purchaseServices;

    public static void Init(String listenerObjectName)
    {
        _purchaseServices = new SPPurchaseNativeServices(listenerObjectName, false);
    }

    public static void InitWithLogs(String listenerObjectName)
    {
        _purchaseServices = new SPPurchaseNativeServices(listenerObjectName, true);
    }

    public static void RequestProductData(String productIds)
    {
        if(_purchaseServices != null)
        {
            String[] ids = productIds.split(",");
            ArrayList<String> skus  = new ArrayList<String>();
            for (String p : ids)
            {
                skus.add(p);
            }
            _purchaseServices.loadProducts(skus);
        }
    }

    public static void EnableHighDetailLogs(boolean shouldEnable)
    {
        if(_purchaseServices != null)
        {
            _purchaseServices.enableHighDetailLogs(shouldEnable);
        }
    }

    public static void PurchaseProduct(String productIdentifier)
    {
        //Possible debug ids:
        //android.test.purchased
        //android.test.canceled
        //android.test.refunded
        //android.test.item_unavailable
        if(_purchaseServices != null)
        {
            _purchaseServices.purchaseProduct(productIdentifier);
        }
    }

    public static void FinishPendingTransaction(String productIdentifier)
    {
        if(_purchaseServices != null)
        {
            _purchaseServices.finishPendingTransaction(productIdentifier);
        }
    }

    public static void ForceFinishPendingTransactions()
    {
        if(_purchaseServices != null)
        {
            _purchaseServices.forceFinishPendingTransactions();
        }
    }

    public static void Unbind()
    {
        if(_purchaseServices != null)
        {
            _purchaseServices.unbind();
        }
    }
}
