package es.socialpoint.sparta.purchase;

import android.app.Activity;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.Log;
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

public class SPPurchaseNativeServices extends Activity implements IabBroadcastListener {

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


    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SPPurchaseNativeServices.instance = this;
    }

    // We're being destroyed. It's important to dispose of the helper here!
    @Override
    public void onDestroy() {
        super.onDestroy();

        // very important:
        if (_broadcastReceiver != null) {
            unregisterReceiver(_broadcastReceiver);
        }

        // very important:
        Log.d(TAG, "Destroying helper.");
        if (_helper != null) {
            _helper.disposeWhenFinished();
            _helper = null;
        }
    }

    public void Init(String listenerObjectName)
    {
        _unityMessageSender = new UnityGameObject(listenerObjectName);
        _unityMessageSender.SendMessage("StoreDebugLog", "*** TEST Hello World");

        _highDetailedLogEnabled = true;
        _setupReady = false;

        /*
        String base64EncodedPublicKey = "CONSTRUCT_YOUR_KEY_AND_PLACE_IT_HERE";

        // Create the helper, passing it our context and the public key to verify signatures with
        Log.d(TAG, "Creating IAB helper.");
        _helper = new IabHelper(this, base64EncodedPublicKey);

        // enable debug logging (for a production application, you should set this to false).
        _helper.enableDebugLogging(_highDetailedLogEnabled);

        // Start setup. This is asynchronous and the specified listener
        // will be called once setup completes.
        Log.d(TAG, "Starting setup.");
        _helper.startSetup(new IabHelper.OnIabSetupFinishedListener() {
            public void onIabSetupFinished(IabResult result) {
                Log.d(TAG, "Setup finished.");

                if (!result.isSuccess()) {
                    // Oh noes, there was a problem.
                    detailedLog("Problem setting up in-app billing: " + result);
                    return;
                }
                // Have we been disposed of in the meantime? If so, quit.
                if (_helper == null) return;

                _setupReady = true;

                // Important: Dynamically register for broadcast messages about updated purchases.
                // We register the receiver here instead of as a <receiver> in the Manifest
                // because we always call getPurchases() at startup, so therefore we can ignore
                // any broadcasts sent while the app isn't running.
                // Note: registering this listener in an Activity is a bad idea, but is done here
                // because this is a SAMPLE. Regardless, the receiver must be registered after
                // IabHelper is setup, but before first call to getPurchases().
                _broadcastReceiver = new IabBroadcastReceiver(SPPurchaseNativeServices.this);
                IntentFilter broadcastFilter = new IntentFilter(IabBroadcastReceiver.ACTION);
                registerReceiver(_broadcastReceiver, broadcastFilter);

                // IAB is fully set up.
            }
        });
        */
    }

    public void LoadProducts()
    {
        if(!IsHelperReady()) {
            _unityMessageSender.SendMessage("OnQueryInventoryFailed", "Setup not ready");
            return;
        }

        detailedLog("Products Request Started");
        try {
            _helper.queryInventoryAsync(_gotInventoryListener);
        } catch (IabAsyncInProgressException e) {
            String errorMessage = "Products Request Cancelled: Another async operation in progress";
            detailedLog(errorMessage);
            _unityMessageSender.SendMessage("OnQueryInventoryFailed", errorMessage);
        }
    }

    private boolean IsHelperReady() {
        return _setupReady && (_helper != null);
    }


    /* Product Operations */

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

            List<SkuDetails> skus = inventory.getAllSkuDetails();
            _unityMessageSender.SendMessage("OnQueryInventorySucceeded", GetProductsJson(skus));


            /*
             * Check for items we own. Notice that for each purchase, we check
             * the developer payload to see if it's correct! See
             * verifyDeveloperPayload().
             */
            //TODO: Get purchases from inventory
        }
    };
}
