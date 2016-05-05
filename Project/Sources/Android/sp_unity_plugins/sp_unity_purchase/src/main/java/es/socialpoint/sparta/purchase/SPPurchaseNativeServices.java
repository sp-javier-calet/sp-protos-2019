package es.socialpoint.sparta.purchase;

import android.content.Intent;
import android.content.IntentFilter;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.util.List;

import es.socialpoint.unity.base.SPUnityActivityEventListener;
import es.socialpoint.unity.base.SPUnityActivityEventManager;
import es.socialpoint.unity.base.UnityGameObject;

import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver;
import es.socialpoint.sparta.purchase.utils.IabBroadcastReceiver.IabBroadcastListener;
import es.socialpoint.sparta.purchase.utils.IabHelper;
import es.socialpoint.sparta.purchase.utils.IabHelper.IabAsyncInProgressException;
import es.socialpoint.sparta.purchase.utils.IabResult;
import es.socialpoint.sparta.purchase.utils.Inventory;
import es.socialpoint.sparta.purchase.utils.Purchase;
import es.socialpoint.sparta.purchase.utils.SkuDetails;

public class SPPurchaseNativeServices implements IabBroadcastListener, SPUnityActivityEventListener {

    // Type values must match the possible states defined by google
    //http://developer.android.com/intl/es/google/play/billing/billing_reference.html
    static class PurchaseState {
        public static final int Purchased = 0;
        public static final int Canceled = 1;
        public static final int Refunded = 2;
    }

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

    // Updated inventory
    private Inventory _inventory;

    private boolean _highDetailedLogEnabled;

    private boolean _setupReady;

    public SPPurchaseNativeServices(String listenerObjectName)
    {
        _unityMessageSender = new UnityGameObject(listenerObjectName);
        _unityMessageSender.SendMessage("StoreDebugLog", "*** TEST Hello World");

        _highDetailedLogEnabled = true;
        _setupReady = false;

        //*** TEST Possible to ignore public key?? Or should be good to set it and use it??
        String base64EncodedPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAxvk2mHxFc+WpJojVkT+3Sh62zsfHT91bDKsxHH3JM6RSi72a5ynCrIhAzGckH0mjNafvEh0Bf1m3T0XF+Wk8fBCXXKZSmLz85A7VX80RF0oBlo0d+QCvrafgSHWy8XsZ45hQPIN9hvfcGnx4zqJjsGVKin5WGH48cGCS3R/O3pXNuuQqLZ3TaI34yOVmg+Ov2nzgl1VFGjiepEiIeOqqs/Usg0OIEbDRdQc/Nl1bbXw6vW0tF7amEdeTKk7pCloKIaLm7kA9H7txa/3JKge+NkZJN8JIKc4LEZ57PFz+7+ayPd42GTmfUaO16saE7JEw8tWJ5dOopGfNa2FdhfFJiQIDAQAB";

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
                SPUnityActivityEventManager.Register(SPPurchaseNativeServices.this);
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
    }

    private boolean IsHelperReady() {
        return _setupReady && (_helper != null);
    }


    /* Product Operations */

    public void LoadProducts(final List<String> productIds)
    {
        detailedLog("Products Request Started");

        if(!IsHelperReady()) {
            _unityMessageSender.SendMessage("OnQueryInventoryFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    _helper.queryInventoryAsync(true, productIds, null, _gotInventoryListener);
                } catch (IabAsyncInProgressException e) {
                    String errorMessage = "Products Request Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnQueryInventoryFailed", errorMessage);
                }
            }
        });
    }

    public void PurchaseProduct(final String productIdentifier)
    {
        detailedLog("Product Purchase Started: " + productIdentifier);

        if(!IsHelperReady()) {
            _unityMessageSender.SendMessage("OnPurchaseFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    int requestCode = 123;//Arbitrary
                    _helper.launchPurchaseFlow(UnityPlayer.currentActivity, productIdentifier, requestCode, _purchaseFinishedListener);
                } catch (IabAsyncInProgressException e) {
                    String errorMessage = "Product Purchase Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnPurchaseFailed", errorMessage);
                }
            }
        });
    }

    private String GetProductJson(SkuDetails product)
    {
        String json = "{"
                + dictionaryKeyFormat("itemType") + dictionaryStringValueFormat(product.getItemType(), false)
                + dictionaryKeyFormat("sku") +  dictionaryStringValueFormat(product.getSku(), false)
                + dictionaryKeyFormat("type") + dictionaryStringValueFormat(product.getType(), false)
                + dictionaryKeyFormat("price") + dictionaryStringValueFormat(product.getPrice(), false)
                + dictionaryKeyFormat("title") +  dictionaryStringValueFormat(product.getTitle(), false)
                + dictionaryKeyFormat("description") +  dictionaryStringValueFormat(product.getDescription(), false)
                + dictionaryKeyFormat("currencyCode") +  dictionaryStringValueFormat(product.getPriceCurrencyCode(), false)
                + dictionaryKeyFormat("priceValue") + dictionaryLongValueFormat(product.getPriceAmountMicros(), true)
                + "}";
        return json;
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

    /* Transaction Operations */

    public void FinishPendingTransaction(final String productIdentifier)
    {
        detailedLog("Finishing Transaction: " + productIdentifier);

        if(!IsHelperReady() || _inventory == null) {
            _unityMessageSender.SendMessage("OnConsumePurchaseFailed", "Setup not ready");
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    if(_inventory.hasPurchase(productIdentifier)) {
                        Purchase purchase = _inventory.getPurchase(productIdentifier);
                        _helper.consumeAsync(purchase, _consumeFinishedListener);
                    }
                    else {
                        _unityMessageSender.SendMessage("OnConsumePurchaseFailed", "Invalid transaction data");
                    }
                } catch (IabAsyncInProgressException e) {
                    String errorMessage = "Consume Product Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnConsumePurchaseFailed", errorMessage);
                }
            }
        });
    }

    public void ForceFinishPendingTransactions()
    {
        detailedLog("Forcefull Finishing All Transactions.");

        if(!IsHelperReady() || _inventory == null) {
            return;
        }

        UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
            public void run() {
                try {
                    List<Purchase> allTransactions = _inventory.getAllPurchases();
                    _helper.consumeAsync(allTransactions, _consumeMultiFinishedListener);
                } catch (IabAsyncInProgressException e) {
                    String errorMessage = "Consume Product Cancelled: Another async operation in progress";
                    detailedLog(errorMessage);
                    _unityMessageSender.SendMessage("OnConsumePurchaseFailed", errorMessage);
                }
            }
        });
    }

    private void UpdateTransaction(Purchase purchase)
    {
        if(purchase == null)
        {
            return;
        }

        switch (purchase.getPurchaseState())
        {
            case PurchaseState.Purchased:
            {
                detailedLog("Purchase successful: " + purchase.getSku());
                _unityMessageSender.SendMessage("OnPurchaseSucceeded", GetTransactionJson(purchase));
            }
                break;
            case PurchaseState.Canceled:
            {
                String message = "Purchase canceled: " + purchase.getSku();
                detailedLog(message);
                _unityMessageSender.SendMessage("OnPurchaseFailed", message);
            }
                break;
            default:
                break;
        }
    }

    private String GetTransactionJson(Purchase purchase)
    {
        String json = "{"
                + dictionaryKeyFormat("itemType") + dictionaryStringValueFormat(purchase.getItemType(), false)
                + dictionaryKeyFormat("orderId") + dictionaryStringValueFormat(purchase.getOrderId(), false)
                + dictionaryKeyFormat("packageName") + dictionaryStringValueFormat(purchase.getPackageName(), false)
                + dictionaryKeyFormat("sku") + dictionaryStringValueFormat(purchase.getSku(), false)
                + dictionaryKeyFormat("purchaseTime") +  dictionaryLongValueFormat(purchase.getPurchaseTime(), false)
                + dictionaryKeyFormat("purchaseState") +  dictionaryIntValueFormat(purchase.getPurchaseState(), false)
                + dictionaryKeyFormat("developerPayload") +  dictionaryStringValueFormat(purchase.getDeveloperPayload(), false)
                + dictionaryKeyFormat("token") +  dictionaryStringValueFormat(purchase.getToken(), false)
                + dictionaryKeyFormat("originalJson") +  dictionaryRawValueFormat(purchase.getOriginalJson(), false)
                + dictionaryKeyFormat("signature") + dictionaryStringValueFormat(purchase.getSignature(), true)
                + "}";
        return json;
    }

    private String GetTransactionsJson(List<Purchase> purchases)
    {
        int count = 0;
        String json = "[";
        for (Purchase p : purchases) {
            if(count > 0)
            {
                json += ",";
            }
            ++count;
            json += GetTransactionJson(p);
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

    String dictionaryKeyFormat(String key)
    {
        return "\"" + key + "\":";
    }

    String dictionaryStringValueFormat(String value, boolean isFinalValue)
    {
        return "\"" + value + "\"" + getConcatString(isFinalValue);
    }

    String dictionaryRawValueFormat(String value, boolean isFinalValue)
    {
        return value + getConcatString(isFinalValue);
    }

    String dictionaryLongValueFormat(Long value, boolean isFinalValue)
    {
        return Long.toString(value) + getConcatString(isFinalValue);
    }

    String dictionaryIntValueFormat(int value, boolean isFinalValue)
    {
        return Integer.toString(value) + getConcatString(isFinalValue);
    }

    String getConcatString(boolean isFinalValue)
    {
        String concatString = "";
        if(!isFinalValue)
        {
            concatString += ",";
        }
        return concatString;
    }

    /* Listeners */

    @Override
    public void receivedBroadcast()
    {
        // Received a broadcast notification that the inventory of items has changed
        detailedLog("Received broadcast notification. Querying inventory.");
        //TODO: Is this intended for updated products or purchases??
        //LoadProducts();
    }

    @Override
    public void HandleActivityResult(int requestCode, int resultCode, Intent data)
    {
        if(IsHelperReady())
        {
           _helper.handleActivityResult(requestCode, resultCode, data);
        }
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
            _inventory = inventory;

            // Loaded products
            List<SkuDetails> skus = inventory.getAllSkuDetails();
            _unityMessageSender.SendMessage("OnQueryInventorySucceeded", GetProductsJson(skus));

            //Pending purchases
            List<Purchase> purchases = inventory.getAllPurchases();
            for (Purchase p : purchases) {
                UpdateTransaction(p);
            }
        }
    };

    // Callback for when a purchase is finished
    IabHelper.OnIabPurchaseFinishedListener _purchaseFinishedListener = new IabHelper.OnIabPurchaseFinishedListener() {
        public void onIabPurchaseFinished(IabResult result, Purchase purchase) {
            detailedLog("Purchase finished: " + result + ", purchase: " + purchase);
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            if (result.isFailure() || purchase == null) {
                detailedLog("Purchase failed: " + result);
                _unityMessageSender.SendMessage("OnPurchaseFailed", result.toString());
                return;
            }

            //TODO: Add purchase to _inventory? or does it updates with the receivedBroadcast listener?
            //TODO: Maybe we should add it if it doesn't have it, just in case the update is slow
            UpdateTransaction(purchase);
        }
    };

    // Called when consumption is complete
    IabHelper.OnConsumeFinishedListener _consumeFinishedListener = new IabHelper.OnConsumeFinishedListener() {
        public void onConsumeFinished(Purchase purchase, IabResult result) {
            detailedLog("Consumption finished. Purchase: " + purchase + ", result: " + result);
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            if (result.isFailure() || purchase == null) {
                detailedLog("Consume failed: " + result);
                _unityMessageSender.SendMessage("OnConsumePurchaseFailed", result.toString());
                return;
            }

            detailedLog("Consume successful: " + purchase.getSku());
            _unityMessageSender.SendMessage("OnConsumePurchaseSucceeded", GetTransactionJson(purchase));
        }
    };

    // Callback that notifies when a multi-item consumption operation finishes.
    IabHelper.OnConsumeMultiFinishedListener _consumeMultiFinishedListener = new IabHelper.OnConsumeMultiFinishedListener() {
        public void onConsumeMultiFinished(List<Purchase> purchases, List<IabResult> results) {
            detailedLog("Multi Consumption Finished.");
            // if we were disposed of in the meantime, quit.
            if (_helper == null) return;

            int totalPurchases = purchases.size();
            int totalResults = results.size();
            if(totalPurchases != totalResults) return;

            for(int i = 0; i < totalPurchases; ++i)
            {
                _consumeFinishedListener.onConsumeFinished(purchases.get(i), results.get(i));
            }
        }
    };
}
