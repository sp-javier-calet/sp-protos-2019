package es.socialpoint.sparta.purchase;

import android.app.Activity;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayerNativeActivity;

import java.util.ArrayList;
import java.util.List;

import es.socialpoint.unity.base.UnityGameObject;

import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver;
import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver.IabBroadcastListener;
import es.socialpoint.sparta.purchase.utils.IabHelper;
import es.socialpoint.sparta.purchase.utils.IabHelper.IabAsyncInProgressException;
import es.socialpoint.sparta.purchase.utils.IabResult;
import es.socialpoint.sparta.purchase.utils.Inventory;
import es.socialpoint.sparta.purchase.utils.Purchase;
import es.socialpoint.sparta.purchase.utils.SkuDetails;

public class SPPurchaseNativeServices implements IabBroadcastListener {

    //Instance reference
    public static SPPurchaseNativeServices instance;

    // Debug tag, for logging
    private static final String TAG = "[SP-IAP]";

    //Class to send messages to Unity
    private UnityGameObject _unityMessageSender;

    // The helper object
    private IabHelper _helper;

    // Provides purchase notification while this app is running
    private IabBroadcastReceiver _broadcastReceiver;

    private boolean _highDetailedLogEnabled;

    private boolean _setupReady;

    public SPPurchaseNativeServices(String listenerObjectName)
    {
        _unityMessageSender = new UnityGameObject(listenerObjectName);
        _unityMessageSender.SendMessage("StoreDebugLog", "*** TEST Hello World");

        _highDetailedLogEnabled = true;
        _setupReady = false;

        //*
        String base64EncodedPublicKey = "CONSTRUCT_YOUR_KEY_AND_PLACE_IT_HERE";

        // Create the helper, passing it our context and the public key to verify signatures with
        detailedLog("Creating IAB helper.");
        _helper = new IabHelper(UnityPlayer.currentActivity, base64EncodedPublicKey);

        // enable debug logging (for a production application, you should set this to false).
        _helper.enableDebugLogging(_highDetailedLogEnabled);

        // Start setup. This is asynchronous and the specified listener
        // will be called once setup completes.
        detailedLog("Starting setup.");
        _helper.startSetup(new IabHelper.OnIabSetupFinishedListener() {
            public void onIabSetupFinished(IabResult result) {
                detailedLog("Setup finished.");
                // Have we been disposed of in the meantime? If so, quit.
                if (_helper == null) return;

                if (!result.isSuccess()) {
                    // Oh noes, there was a problem.
                    String errorMessage = "Problem setting up in-app billing: " + result;
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnBillingNotSupported", errorMessage);
                    return;
                }

                _setupReady = true;
                _unityMessageSender.SendMessage("OnBillingSupported", "");

                // Important: Dynamically register for broadcast messages about updated purchases.
                // We register the receiver here instead of as a <receiver> in the Manifest
                // because we always call getPurchases() at startup, so therefore we can ignore
                // any broadcasts sent while the app isn't running.
                // Note: registering this listener in an Activity is a bad idea, but is done here
                // because this is a SAMPLE. Regardless, the receiver must be registered after
                // IabHelper is setup, but before first call to getPurchases().
                _broadcastReceiver = new IabBroadcastReceiver(SPPurchaseNativeServices.this);
                IntentFilter broadcastFilter = new IntentFilter(IabBroadcastReceiver.ACTION);
                UnityPlayer.currentActivity.registerReceiver(_broadcastReceiver, broadcastFilter);

                // IAB is fully set up.
            }
        });
        //*/
    }

    private boolean IsHelperReady() {
        return _setupReady && (_helper != null);
    }


    /* Product Operations */

    public void LoadProducts()
    {
        detailedLog("Products Request Started");

        if(!IsHelperReady()) {
            _unityMessageSender.SendMessage("OnQueryInventoryFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    //*** TEST (pass as final param)
                    ArrayList<String> productIds = new ArrayList<String>();
                    productIds.add(0, "iap_1");

                    _helper.queryInventoryAsync(true, productIds, null, _gotInventoryListener);
                } catch (IabAsyncInProgressException e) {
                    String errorMessage = "Products Request Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnQueryInventoryFailed", errorMessage);
                }
            }
        });
    }

    private String GetProductJson(SkuDetails product)
    {
        /*String json = "{"
                + "itemType:" + product.getItemType() + ","
                + "sku:" +  product.getSku() + ","
                + "type:" + product.getType() + ","
                + "price:" + product.getPrice()  + ","
                + "title:" +  product.getTitle() + ","
                + "description:" +  product.getDescription() + ","
                + "currencyCode:" +  product.getPriceCurrencyCode() + ","
                + "priceValue:" + product.getPriceAmountMicros()
                + "}";
        return json;*/
        return product.toString();
    }

    private String GetProductsJson(List<SkuDetails> products)
    {
        int count = 0;
        String json = "[";
        for (SkuDetails p : products) {
            if(count > 0)
            {
                json += ",";
            }
            ++count;
            json += GetProductJson(p);
        }
        json += "]";
        return json;
    }

    /* Debug */

    void detailedLog(String message) {
        if(_highDetailedLogEnabled) {
            Log.d(TAG, message);
        }
    }

    /* Listeners */

    @Override
    public void receivedBroadcast() {
        // Received a broadcast notification that the inventory of items has changed
        detailedLog("Received broadcast notification. Querying inventory.");
        LoadProducts();
    }

    // Listener that's called when we finish querying the items and subscriptions we own
    IabHelper.QueryInventoryFinishedListener _gotInventoryListener = new IabHelper.QueryInventoryFinishedListener() {
        public void onQueryInventoryFinished(IabResult result, Inventory inventory) {
            detailedLog("Query inventory finished.");
            // Have we been disposed of in the meantime? If so, quit.
            if (_helper == null) return;

            // Is it a failure?
            if (result.isFailure() || inventory == null) {
                detailedLog("Failed to query inventory: " + result);
                _unityMessageSender.SendMessage("OnQueryInventoryFailed", result.toString());
                return;
            }

            detailedLog("Query inventory was successful.");

            //TODO: Filter only ids that the user requested in Unity
            List<SkuDetails> skus = inventory.getAllSkuDetails();
            _unityMessageSender.SendMessage("OnQueryInventorySucceeded", GetProductsJson(skus));

            SkuDetails iap_1 = inventory.getSkuDetails("iap_1");
            if(iap_1 != null) {
                detailedLog("Product 1: " + iap_1.getTitle());
            }


            /*
             * Check for items we own. Notice that for each purchase, we check
             * the developer payload to see if it's correct! See
             * verifyDeveloperPayload().
             */
            //TODO: Get purchases from inventory
        }
    };
}
